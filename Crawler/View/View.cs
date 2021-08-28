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
    private Resource unconsumedResource = null; 

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

    public override void _Process(float delta)
    {
        if (eventQueue.Count > 0)
        {
            this.ClearQueue();
            if (eventQueue.Count == 0)
            {
                queueSync = true;
            }
        }

        if (queueSync && !this.AnyActorAnimating() && eventQueue.Count == 0)
        {
            queueSync = false;
            this.ModelSync();
            Model debugggModel = GetNode<Model>("../Model");
            GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + debugggModel.time + " (sync!)";
        }
    }

    public void ClearQueue()
    {
        while (eventQueue.Count > 0)
        {
            Dictionary ev2 = eventQueue[0];

            // Waiting, dequeuing.
            if (!impatientMode && (int)ev2["subject"] == -1 && (string)ev2["action"] == "Wait")
            {
                if (AnyActorAnimating()) { break; }
            }
            eventQueue.RemoveAt(0);
            if (!impatientMode && (int)ev2["subject"] == -1 && (string)ev2["action"] == "SmallWait")
            {
                break;
            }

            GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + ev2["timestamp"];
            FindNode("WaitPrompt").Set("visible", true);

            // handle the event
            string action = (string)ev2["action"];
            if (new Godot.Directory().FileExists($"res://Crawler/View/Events/{action}Event.gd") ||
                new Godot.Directory().FileExists($"res://Crawler/View/Events/{action}Event.gdc")) // for when you export compiled
            {
                GDScript script = GD.Load<GDScript>($"res://Crawler/View/Events/{action}Event.gd");
                script.New(this, ev2, roles);
                // ev.Free() // resource counted!
                // if event wants to persist, then it creates a node and puts itself in there.
            }

            // TODO: also move below into handling scripts.
            if ((string)ev2["action"] == "Exit" || ((string)ev2["action"] == "Downed" && (int)ev2["subject"] == 0))
            {
                // Temporary!
                GetNode<MessageLog>("UILayer/MessageLog").AnchorTop = 0;
                GetNode<MessageLog>("UILayer/MessageLog").MarginTop = 20;
                GetNode<ColorRect>("UILayer/MessageLog/Background").Color = Color.FromHsv(0, 0, 0);
            }

            // Everything gets sent to the logs.
            GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode("\n * " + ev2["action"] + " " + ev2);
            GetNode<MessageLog>("UILayer/MessageLog").HandleModelEvent(ev2, roles);
        }
    }

    private void ModelSync()
    {
        foreach (Actor a in roles)
        {
            a.ModelSync();
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
