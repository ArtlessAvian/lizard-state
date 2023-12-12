using System;
using System.Collections.Generic;
using Godot;
using LizardState.Engine;

/// <summary>
/// Represents model (floor) information, and how it changes (events, animation).
/// Do not instantiate directly. Load() the scene with Godot.
/// When moving to a new floor, the old model stops being used. Use a new view.
/// </summary>
public partial class View : Node2D
{
    public static Vector2 TILESIZE = new Vector2(24, 16);

    public Model model;
    public bool done = false;

    public bool queueSync = false;
    int viewTime;

    // Godot collections explicitly spelled out.
    // Also feel free to read these from Godot / from an EventHandler

    public Godot.Collections.Dictionary<int, Actor> roles = new Godot.Collections.Dictionary<int, Actor>();
    public Godot.Collections.Dictionary<int, Node2D> items = new Godot.Collections.Dictionary<int, Node2D>();

    // Do not access below collections if you're from Godot / EventHandler.

    public Queue<Reference> eventQueue = new Queue<Reference>(); // If you're reading this you're a Queue<T> ;)
    // Memoized loaded scripts. Values can be null :skull:
    Dictionary<string, GDScript> handlerScripts = new Dictionary<string, GDScript>();
    private List<Reference> runningHandlers = new List<Reference>();
    // All these handlers can accept children (assuming model correctly sends events in order).
    // These are the most recent handlers at each depth.
    private List<Reference> incompleteHandlers = new List<Reference>();

    // super buggy but convenient
    [Export] public bool impatientMode = false;
    [Export] public bool skipAllAnimation = false;

    public void ConnectToModel(Model model)
    {
        this.model = model;

        // Connect to the signal.
        model.Connect("NewEvent", this, "OnModelNewEvent");

        // Create all entities.
        // TODO: Rework things. This is silly.        
        GDScript createScript = GetHandlerScriptOrNull("Create");
        foreach (Entity e in model.GetEntities())
        {
            Godot.Collections.Dictionary fakeEvent = new Godot.Collections.Dictionary() { { "args", e } };
            Reference createEvent = (Reference)createScript.New();
            createEvent.Call("init2", this, fakeEvent);
            createEvent.Call("run");
        }

        GDScript createItemScript = GetHandlerScriptOrNull("CreateItem");
        foreach (FloorItem item in model.GetFloorItems())
        {
            Godot.Collections.Dictionary fakeEvent = new Godot.Collections.Dictionary() { { "args", item } };
            Reference createItemEvent = (Reference)createItemScript.New();
            createItemEvent.Call("init2", this, fakeEvent);
            createItemEvent.Call("run");
        }

        ModelSync();
    }

    public void OnModelNewEvent(Godot.Collections.Dictionary @event)
    {
        // hide stuff on sync.
        TileMap telegraphed = GetNode<TileMap>("Map/Floors/TelegraphedAttacks");
        telegraphed.Hide();

        string action = @event["action"] as string;

        var handlerScript = GetHandlerScriptOrNull(action);
        if (handlerScript is GDScript)
        {
            Reference handlerInstance = (Reference)handlerScript.New();
            handlerInstance.Call("init2", this, @event);
            eventQueue.Enqueue(handlerInstance);
        }
        else
        {
            GD.PushWarning("No handler for event " + action);
        }

        // Everything gets sent to the logs.
        RichTextLabel eventLog = GetNode<RichTextLabel>("UILayer/Debug/EventLog");
        if (eventLog.Text.Contains("YourTurn"))
        {
            eventLog.Clear();
        }

        eventLog.AppendBbcode("\n * " + @event["action"] + " " + @event["subject"]);
        if (@event.Contains("object"))
        {
            eventLog.AppendBbcode(" " + @event["object"]);
        }
    }

    public override void _Process(float delta)
    {
        // Animation Logic
        ClearRunningEvents();
        DequeueAndRunEvents();
        if (queueSync && !this.AnyActorAnimating() && IsQueueClear())
        {
            queueSync = false;
            ModelSync();
        }

        // Other View Logic

        // Debug Panels
        DebugDisplayRunningEvents();
        DebugDisplayAnimatingActors();
    }

    private void ClearRunningEvents()
    {
        // if (!Input.IsActionJustPressed("ui_select")) { return; }

        for (int i = runningHandlers.Count - 1; i >= 0; i--)
        {
            if (runningHandlers[i].Call("is_done") as bool? == true)
            {
                GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + viewTime.ToString();
                runningHandlers.RemoveAt(i);
            }
        }

        if (runningHandlers.Count == 0)
        {
            incompleteHandlers.Clear();
        }
    }

    private void DebugDisplayRunningEvents()
    {
        RichTextLabel runningLabel = GetNode<RichTextLabel>("UILayer/Debug/RunningHandlers");
        runningLabel.Clear();
        for (int i = 0; i < incompleteHandlers.Count; i++)
        {
            Reference handler = incompleteHandlers[i];
            runningLabel.AppendBbcode(new string('-', i) + (handler.GetScript() as Resource).ResourcePath + "\n");
        }
        for (int i = incompleteHandlers.Count; i < 10; i++)
        {
            runningLabel.AppendBbcode("\n");
        }

        foreach (Reference handler in runningHandlers)
        {
            runningLabel.AppendBbcode((handler.GetScript() as Resource).ResourcePath + "\n");
        }
    }

    private void DebugDisplayAnimatingActors()
    {
        RichTextLabel animatingLabel = GetNode<RichTextLabel>("UILayer/Debug/AnimatingActors");
        animatingLabel.Clear();
        foreach (Actor a in roles.Values)
        {
            if (a.IsAnimating())
            {
                animatingLabel.AppendBbcode($"{a.Name} {a.seen} {a.tilePosition.x - Position.x / View.TILESIZE.x} {a.tilePosition.y - Position.y / View.TILESIZE.y} \n");
            }
        }
    }

    public bool IsQueueClear()
    {
        return eventQueue.Count == 0;
    }

    private void DequeueAndRunEvents()
    {
        if (skipAllAnimation)
        {
            eventQueue.Clear();
            return;
        }

        while (!IsQueueClear())
        {
            FindNode("WaitPrompt").Set("visible", true);

            Reference handlerInstance = eventQueue.Peek();

            if (incompleteHandlers.Count > 0 && !impatientMode)
            {
                bool success = false;
                for (int i = incompleteHandlers.Count - 1; i >= 0; i--)
                {
                    if (incompleteHandlers[i].Call("can_accept_child", handlerInstance) as bool? == true)
                    {
                        // slice off [:i]
                        incompleteHandlers.RemoveRange(i + 1, incompleteHandlers.Count - i - 1);
                        success = true;
                        break; // out of for.
                    }
                    if (handlerInstance.Call("can_be_child", incompleteHandlers[i]) as bool? == true)
                    {
                        // slice off [:i]
                        incompleteHandlers.RemoveRange(i + 1, incompleteHandlers.Count - i - 1);
                        success = true;
                        break; // out of for.
                    }
                }
                if (!success)
                {
                    break; // out of while.
                }
            }

            handlerInstance.Call("run");
            if (handlerInstance.Call("is_done") as bool? == false)
            {
                incompleteHandlers.Add(handlerInstance);
                runningHandlers.Add(handlerInstance);
            }

            viewTime = (int)((Godot.Collections.Dictionary)handlerInstance.Get("event"))["timestamp"];

            eventQueue.Dequeue();
        }
    }

    private GDScript GetHandlerScriptOrNull(string action)
    {
        if (!handlerScripts.ContainsKey(action))
        {
            // honestly kinda dangerous. arbitrary code can run!
            if (new Godot.Directory().FileExists($"res://Crawler/View/Events/{action}Event.gd") ||
                new Godot.Directory().FileExists($"res://Crawler/View/Events/{action}Event.gdc")) // for when you export compiled
            {
                GDScript script = GD.Load<GDScript>($"res://Crawler/View/Events/{action}Event.gd");
                handlerScripts.Add(action, script);
            }
            else
            {
                handlerScripts.Add(action, null);
            }
        }

        return handlerScripts[action];
    }

    public void ModelSync()
    {
        viewTime = model.time;
        GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + viewTime + " (sync!)";

        foreach (Actor a in roles.Values)
        {
            a.ModelSync(viewTime);
        }

        TileMap telegraphed = GetNode<TileMap>("Map/Floors/TelegraphedAttacks");
        telegraphed.Show();
        telegraphed.Clear();
        foreach (Actor a in roles.Values)
        {
            // Hide intent if it does not hit between player turn and player's next turn.
            // TODO: Assumes player has id 0.
            if (a.role.nextMove > roles[0].role.nextMove + 1)
            {
                continue;
            }
            if (a.role.nextMove >= roles[0].role.nextMove + 1 && a.role.id > roles[0].role.id)
            {
                continue;
            }

            if (a.role.queuedAction?.GetTargetPos(a.role.position) is AbsolutePosition target)
            {
                if (a.role.queuedAction.TargetingType is TargetingType.Ray ray)
                {
                    foreach (AbsolutePosition pos in GridHelper.LineBetween(a.role.position, target))
                    {
                        telegraphed.SetCell(pos.x, pos.y, 3);
                    }
                }
                else if (a.role.queuedAction.TargetingType is TargetingType.Line line)
                {
                    foreach (AbsolutePosition pos in GridHelper.LineBetween(a.role.position, target))
                    {
                        telegraphed.SetCell(pos.x, pos.y, 3);
                    }
                }
                else if (a.role.queuedAction.TargetingType is TargetingType.Cone cone)
                {
                    foreach (AbsolutePosition pos in VisibilityTrie.ConeOfView(a.role.position, x => false, a.role.queuedAction.Range.max, target - a.role.position, cone.sectorDegrees))
                    {
                        telegraphed.SetCell(pos.x, pos.y, 3);
                    }
                }
                else if (a.role.queuedAction.TargetingType is TargetingType.Smite smite)
                {
                    foreach (AbsolutePosition pos in VisibilityTrie.FieldOfView(target, x => false, smite.radius))
                    {
                        telegraphed.SetCell(pos.x, pos.y, 3);
                    }
                }
            }
        }

        FogOfWarSystem fog = model.GetSystem<FogOfWarSystem>();
        MapView mapView = GetNode<MapView>("Map");
        mapView.ModelSync(model.map, fog);
    }

    private bool AnyActorAnimating()
    {
        foreach (Actor a in roles.Values)
        {
            if (a.IsAnimating())
            {
                return true;
            }
        }
        return false;
    }

    public override void _UnhandledInput(InputEvent ev)
    {
        if (ev is InputEventKey eventKey && eventKey.Pressed && !eventKey.IsEcho())
        {
            if (eventKey.Scancode == (int)KeyList.Quoteleft)
            {
                impatientMode = !impatientMode;
                foreach (Actor a in roles.Values)
                {
                    a.impatientMode = impatientMode;
                }
                // Engine.TimeScale = impatientMode ? 2 : 1;
                string thing = (impatientMode ? "on" : "off");

                GetNode<RichTextLabel>("UILayer/MessageLog").AppendBbcode($"\n * Impatient mode {thing}!");
            }

            if (eventKey.Scancode == (int)KeyList.F1)
            {
                Control eventLog = FindNode("EventLog") as Control;
                eventLog.Visible = !eventLog.Visible;
            }

            if (eventKey.Scancode == (int)KeyList.F2)
            {
                Control eventLog = FindNode("RunningHandlers") as Control;
                eventLog.Visible = !eventLog.Visible;
            }

            if (eventKey.Scancode == (int)KeyList.F3)
            {
                Control eventLog = FindNode("AnimatingActors") as Control;
                eventLog.Visible = !eventLog.Visible;
            }
        }
    }
}
