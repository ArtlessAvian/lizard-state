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
    private bool queueSync = false;
    private Resource unconsumedEvent = null; 

    int viewTime;

    // conveniences
    [Export] public Actor playerActor;
    // super buggy but convenient
    [Export] public bool impatientMode = false;

    public override void _Ready()
    {
        // get the model
        // for every entity in the model
            // create that entity.
        // subscribe to the event thing.
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
            if (IsQueueClear())
            {
                queueSync = true;
            }
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
        return eventQueue.Count == 0 && unconsumedEvent == null;
    }

    public void ClearQueue()
    {
        while (!IsQueueClear())
        {
            FindNode("WaitPrompt").Set("visible", true);

            Dictionary ev = eventQueue[0];
            // Create the eventhandler
            if (unconsumedEvent == null)
            {
                string action = ev["action"] as string;
                // honestly kinda dangerous. arbitrary code can run!
                if (new Godot.Directory().FileExists($"res://Crawler/View/Events/{action}Event.gd") ||
                    new Godot.Directory().FileExists($"res://Crawler/View/Events/{action}Event.gdc")) // for when you export compiled
                {
                    GDScript script = GD.Load<GDScript>($"res://Crawler/View/Events/{action}Event.gd");
                    unconsumedEvent = script.New(this, ev, roles) as Resource;
                }
            }

            // Run the eventhandler
            if (unconsumedEvent != null)
            {
                unconsumedEvent.Call("run", this, ev, roles);

                // If you can't consume. Weird, but eh.
                object canConsume = unconsumedEvent.Call("can_consume");
                // GD.Print("checking", canConsume);
                if (canConsume is bool eeee && !eeee)
                {
                    // GD.Print("done waiting");
                    break;
                }    
            }

            // consume the event
            eventQueue.RemoveAt(0);
            unconsumedEvent = null;

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
            GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode("\n * " + ev["action"] + " " + ev);
            GetNode<MessageLog>("UILayer/MessageLog").HandleModelEvent(ev, roles);
        }
    }

    private void ModelSync()
    {
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
