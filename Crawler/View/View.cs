using Godot;
using Godot.Collections;

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
    public Array<Dictionary> eventQueue = new Array<Dictionary>();
    public Dictionary<int, Actor> roles = new Dictionary<int, Actor>();

    // Animation Statefulness
    public bool queueSync = false;

    Dictionary<string, GDScript> handlerScripts = new Dictionary<string, GDScript>();
    private Reference handlerInstance = null;
    Array<Reference> runningHandlers = new Array<Reference>();

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

        VisionSystem vision = model.GetSystem<VisionSystem>();
        FogOfWarSystem fog = model.GetSystem<FogOfWarSystem>();

        // Copy map knowledge.
        MapView mapView = GetNode<MapView>("Map");
        mapView.ModelSync(model.Map, fog);

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
        eventQueue.Add(@event);

        // Everything gets sent to the logs.
        if (GetNode<RichTextLabel>("UILayer/DebugLog").Visible)
        {
            GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode("\n * " + @event["action"] + " " + @event["subject"]);
            if (@event.Contains("object"))
            {
                GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode(" " + @event["object"]);
            }
        }
    }

    public override void _Process(float delta)
    {
        RichTextLabel runningLabel = GetNode<RichTextLabel>("UILayer/RunningHandlers");
        runningLabel.Clear();
        foreach (Reference handler in runningHandlers)
        {
            runningLabel.AppendBbcode((handler.GetScript() as Resource).ResourcePath + "\n");
        }

        for (int i = runningHandlers.Count - 1; i >= 0; i--)
        {
            if (runningHandlers[i].Call("is_done") as bool? == true)
            {
                runningHandlers.RemoveAt(i);
            }
        }

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
    }

    public bool IsQueueClear()
    {
        return eventQueue.Count == 0;
    }

    public void ClearQueue()
    {
        if (skipAllAnimation)
        {
            eventQueue.Clear();
            return;
        }

        while (!IsQueueClear())
        {
            FindNode("WaitPrompt").Set("visible", true);

            Dictionary ev = eventQueue[0];
            string action = ev["action"] as string;

            // Try to create the handler, if not already created
            if (handlerInstance is null)
            {
                var handlerScript = GetHandlerScriptOrNull(action);
                if (handlerScript is GDScript)
                {
                    handlerInstance = (Reference)handlerScript.New();
                    handlerInstance.Call("init2", this, ev);
                }
            }

            // todon't: merge this with previous if statement. the logic is different.
            if (handlerInstance is object)
            {
                if (runningHandlers.Count > 0)
                {
                    object can_concurrent = handlerInstance.Call("can_run_concurrently_with", runningHandlers);
                    if (!impatientMode && can_concurrent as bool? == false)
                    {
                        break; // out of the while
                    }
                }

                handlerInstance.Call("run");
                if (handlerInstance.Call("is_done") as bool? == false)
                {
                    runningHandlers.Add(handlerInstance);
                }
            }

            // consume the event
            eventQueue.RemoveAt(0);
            handlerInstance = null;

            viewTime = (int)ev["timestamp"];
            GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + viewTime.ToString();
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

        FogOfWarSystem fog = model.GetSystem<FogOfWarSystem>();
        MapView mapView = GetNode<MapView>("Map");
        mapView.ModelSync(model.Map, fog);
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
}
