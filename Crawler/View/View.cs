using Godot;
using Godot.Collections;

// Holds a model and shows what's happening.
public partial class View : Node2D
{
    public static Vector2 TILESIZE = new Vector2(24, 16);

    // Possibly bad performance on dequeue. Not relevant yet.
    public Array<Dictionary> eventQueue = new Array<Dictionary>();
    // Could be an dictionary?
    public Array<Actor> roles = new Array<Actor>();

    // Animation Statefulness
    public bool queueSync = false;

    Dictionary<string, Resource> eventHandlers = new Dictionary<string, Resource>();

    int viewTime;

    // conveniences
    [Export] public Actor playerActor;
    // super buggy but convenient

    [Export] public bool impatientMode = false;
    private Resource initializedHandler = null; 

    public override void _Ready() {}

    public void ConnectToModel(Model model)
    {
        // Connect to the signal.
        model.Connect("NewEvent", this, "OnModelNewEvent");
        
        // Copy the map.

        // Create all entities.
        // TODO: Rework things. This is silly.        
        GDScript createEvent = GD.Load<GDScript>($"res://Crawler/View/Events/CreateEvent.gd");
        foreach (Entity e in model.GetEntities())
        {
            Dictionary fakeEvent = new Dictionary(){{"args", e}};
            createEvent.New(this, fakeEvent, roles);
        }
        
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
        while (!IsQueueClear())
        {
            FindNode("WaitPrompt").Set("visible", true);

            Dictionary ev = eventQueue[0];
            string action = ev["action"] as string;

            // Lazy create the eventhandler
            LazyInitHandler(action);

            // "Initialize" the event if it hasn't been already
            if (initializedHandler is null && eventHandlers[action] is Resource handler)
            {
                // todo: rename first_run to reinit
                handler.Call("first_run", this, ev, roles);
                initializedHandler = handler;
            }

            // todon't: merge this with previous if statement. the logic is different.
            if (initializedHandler is object)
            {
                object shouldWaitBefore = initializedHandler.Call("should_wait_before", this, ev);
                if (!impatientMode && shouldWaitBefore is bool aaaa && !aaaa)
                {
                    // GD.Print("waiting before");
                    break;
                }

                initializedHandler.Call("run", this, ev, roles);

                object can_consume = initializedHandler.Call("can_consume");
                if (!impatientMode && can_consume is bool eeee && !eeee)
                {
                    // GD.Print("waiting during/after");
                    break;
                }
            }

            // consume the event
            eventQueue.RemoveAt(0);
            initializedHandler = null;

            viewTime = (int)ev["timestamp"];
            GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + viewTime.ToString();

            // TODO: also move below into handling scripts.
            if ((string)ev["action"] == "Exit" || ((string)ev["action"] == "Downed" && (int)ev["subject"] == 0))
            {
                // Temporary!
                GetNode<MessageLog>("UILayer/MessageLog").AnchorTop = 0;
                GetNode<MessageLog>("UILayer/MessageLog").MarginTop = 20;
                GetNode<ColorRect>("UILayer/MessageLog/Background").Color = Color.FromHsv(0, 0, 0);
            }

            // Everything gets sent to the logs.
            if (GetNode<RichTextLabel>("UILayer/DebugLog").Visible)
            {
                GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode("\n * " + ev["action"] + " " + ev);
            }
            GetNode<MessageLog>("UILayer/MessageLog").HandleModelEvent(ev, roles);
        }
    }

    private void LazyInitHandler(string action)
    {
        if (!eventHandlers.ContainsKey(action))
        {
            // honestly kinda dangerous. arbitrary code can run!
            if (new Godot.Directory().FileExists($"res://Crawler/View/Events/{action}Event.gd") ||
                new Godot.Directory().FileExists($"res://Crawler/View/Events/{action}Event.gdc")) // for when you export compiled
            {
                GDScript script = GD.Load<GDScript>($"res://Crawler/View/Events/{action}Event.gd");
                eventHandlers.Add(action, script.New() as Resource);
            }
            else
            {
                eventHandlers.Add(action, null);
            }
        }
    }

    private void ModelSync()
    {
        // does not sync map.
        foreach (Actor a in roles)
        {
            a.ModelSync(viewTime);
        }
    }

    private bool AnyActorAnimating()
    {
        foreach (Actor a in roles)
        {
            if (a.IsAnimating())
            {
                return true;
            }
        }
        return false;
    }
}
