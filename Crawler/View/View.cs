using Godot;
using Godot.Collections;
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

    // Possibly bad performance on dequeue. Not relevant yet.
    public Array<Reference> eventQueue = new Array<Reference>();
    public Dictionary<int, Actor> roles = new Dictionary<int, Actor>();
    public Dictionary<int, Node2D> items = new Dictionary<int, Node2D>();

    // Animation Statefulness
    public bool queueSync = false;

    Dictionary<string, GDScript> handlerScripts = new Dictionary<string, GDScript>();
    private Array<Reference> runningHandlers = new Array<Reference>();
    // All these handlers can accept children (assuming model correctly sends events in order).
    // These are the most recent handlers at each depth.
    private Array<Reference> incompleteHandlers = new Array<Reference>();

    int viewTime;

    // conveniences
    [Export] public Actor playerActor;
    // super buggy but convenient

    [Export] public bool impatientMode = false;
    [Export] public bool done = false;
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
            Dictionary fakeEvent = new Dictionary() { { "args", e } };
            Reference createEvent = (Reference)createScript.New();
            createEvent.Call("init2", this, fakeEvent);
            createEvent.Call("run");
        }

        GDScript createItemScript = GetHandlerScriptOrNull("CreateItem");
        foreach (FloorItem item in model.GetFloorItems())
        {
            Dictionary fakeEvent = new Dictionary() { { "args", item } };
            Reference createItemEvent = (Reference)createItemScript.New();
            createItemEvent.Call("init2", this, fakeEvent);
            createItemEvent.Call("run");
        }

        VisionSystem vision = model.GetSystem<VisionSystem>();
        FogOfWarSystem fog = model.GetSystem<FogOfWarSystem>();

        // Copy map knowledge.
        MapView mapView = GetNode<MapView>("Map");
        mapView.ModelSync(model.map, fog);

        // TODO: read the info directly.
        foreach (Entity e in model.GetEntities())
        {
            if (e.providesVision)
            {
                fog.RefreshVision(model, e);
                vision.RefreshVision(model, e);
            }
        }

        viewTime = model.time;

        ModelSync();
    }

    public void OnModelNewEvent(Dictionary @event)
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
            eventQueue.Add(handlerInstance);
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

        // if (Input.IsActionJustPressed("ui_select")) {

        for (int i = runningHandlers.Count - 1; i >= 0; i--)
        {
            if (runningHandlers[i].Call("is_done") as bool? == true)
            {
                viewTime = (int)((Dictionary)runningHandlers[i].Get("event"))["timestamp"];
                GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + viewTime.ToString();
                runningHandlers.RemoveAt(i);
            }
        }

        if (runningHandlers.Count == 0)
        {
            incompleteHandlers.Clear();
        }

        // } // If

        if (!IsQueueClear())
        {
            this.ClearQueue();
        }

        if (queueSync && !this.AnyActorAnimating() && IsQueueClear())
        {
            queueSync = false;

            viewTime = model.time;
            GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + viewTime + " (sync!)";

            this.ModelSync();
        }

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

    private void ClearQueue()
    {
        if (skipAllAnimation)
        {
            eventQueue.Clear();
            return;
        }

        while (!IsQueueClear())
        {
            FindNode("WaitPrompt").Set("visible", true);

            Reference handlerInstance = eventQueue[0];

            if (incompleteHandlers.Count > 0 && !impatientMode)
            {
                bool success = false;
                for (int i = incompleteHandlers.Count - 1; i >= 0; i--)
                {
                    if (incompleteHandlers[i].Call("can_accept_child", handlerInstance) as bool? == true)
                    {
                        // slice off [:i]
                        incompleteHandlers.Resize(i + 1);
                        success = true;
                        break; // out of for.
                    }
                    if (handlerInstance.Call("can_be_child", incompleteHandlers[i]) as bool? == true)
                    {
                        // slice off [:i]
                        incompleteHandlers.Resize(i + 1);
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

            eventQueue.RemoveAt(0);
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
        foreach (Actor a in roles.Values)
        {
            a.ModelSync(viewTime);
        }

        TileMap telegraphed = GetNode<TileMap>("Map/Floors/TelegraphedAttacks");
        telegraphed.Show();
        telegraphed.Clear();
        foreach (Actor a in roles.Values)
        {
            if (a.role.queuedAction?.GetTargetPos(a.role.position) is AbsolutePosition pos)
            {
                telegraphed.SetCell(pos.x, pos.y, 3);
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
