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
    private Resource handlerInstance = null;
    private bool handlerRan = false;

    int viewTime;

    // conveniences
    [Export] public Actor playerActor;
    // super buggy but convenient

    [Export] public bool impatientMode = false;
    [Export] public bool done = false;
    [Export] public bool skipAllAnimation = true;

    private Dictionary previousEvent = new Dictionary() { { "subject", -1 }, { "action", "null" } };

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
            Resource createEvent = (Resource)createScript.New();
            createEvent.Call("init2", this, fakeEvent, null);
            createEvent.Call("setup"); // done in a subclass        
            createEvent.Call("run");
        }

        VisionSystem vision = model.GetNode<VisionSystem>("Systems/Vision");
        FogOfWarSystem fog = model.GetNode<FogOfWarSystem>("Systems/Fog");

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
            GD.Print(e.ResourcePath);
        }

        viewTime = model.time;

        ModelSync();
    }

    public void OnModelNewEvent(Dictionary @event)
    {
        eventQueue.Add(@event);
    }

    public override void _Process(float delta)
    {
        if (!IsQueueClear())
        {
            this.ClearQueue();
        }

        if (queueSync && !this.AnyActorAnimating() && IsQueueClear())
        {
            queueSync = false;

            Model debugggModel = GetNode<Model>("../Model");
            viewTime = debugggModel.time;
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

            // "Initialize" the event if it hasn't been already
            if (handlerInstance is null)
            {
                // (do not compress for clarity)
                // (also i plan to yeet this code anyways)
                var handlerScript = GetHandlerScriptOrNull(action);
                if (handlerScript is GDScript)
                {
                    handlerInstance = (Resource)handlerScript.New(); // called in EventHandler.gd
                    handlerInstance.Call("init2", this, ev, null);
                    handlerInstance.Call("setup"); // done in a subclass
                    handlerRan = false;
                }
            }

            // todon't: merge this with previous if statement. the logic is different.
            // also todo: ugly logic.
            if (handlerInstance is object && !handlerRan)
            {
                object shouldWaitBefore = handlerInstance.Call("should_wait_before");
                if (!impatientMode && shouldWaitBefore is bool aaaa && aaaa)
                {
                    break; // out of the while
                }

                handlerInstance.Call("run");
                handlerRan = true;
            }

            // consume the event
            eventQueue.RemoveAt(0);
            handlerInstance = null;
            handlerRan = false;
            previousEvent = ev;

            viewTime = (int)ev["timestamp"];
            GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + viewTime.ToString();

            // TODO: ASAP for cleanliness!!!!!! eughuegheu.
            // TODO: also move below into handling scripts.
            if ((string)ev["action"] == "Exit" || ((string)ev["action"] == "Downed" && (int)ev["subject"] == 0))
            {
                // Temporary!

            }

            // Everything gets sent to the logs.
            if (GetNode<RichTextLabel>("UILayer/DebugLog").Visible)
            {
                GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode("\n * " + ev["action"] + " " + ev["subject"]);
                if (ev.Contains("object"))
                {
                    GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode(" " + ev["object"]);
                }
            }
            GetNode<MessageLog>("UILayer/MessageLog").HandleModelEvent(ev, roles);
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

        FogOfWarSystem fog = model.GetNode<FogOfWarSystem>("Systems/Fog");
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
