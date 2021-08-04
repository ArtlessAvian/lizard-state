using Godot;
using Godot.Collections;

// Holds a model and shows what's happening.
public partial class View : Node2D
{
    public static Vector2 TILESIZE = new Vector2(32, 24);

    // might be bad performance on dequeue.
    // who cares
    public Array<Dictionary> eventQueue = new Array<Dictionary>();
    // Could be an dictionary?
    public Array<Actor> roles = new Array<Actor>();

    // convenience
    [Export] public Actor playerActor;
    // super buggy but convenient
    [Export] public bool impatientMode = false;

    private bool queueSync = false;
    private float modelSyncDelay = 0;

    public override void _Ready()
    {
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
            // // prevents a camera jittering bug.
            // // model syncs before actually done.
            // modelSyncDelay += delta;
            // if (modelSyncDelay > 0.1)
            // {
                // modelSyncDelay = 0;
                queueSync = false;
                this.ModelSync();
                Model debugggModel = GetNode<Model>("../Model");
                GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + debugggModel.time + " (sync!)";
            // }
        }
    }

    public void ClearQueue()
    {
        while (eventQueue.Count > 0)
        {
            Dictionary ev2 = eventQueue[0];
            
            string action = (string)ev2["action"];
            if (new Godot.Directory().FileExists($"res://Crawler/View/Events/{action}Event.gd"))
            {
                GDScript script = GD.Load<GDScript>($"res://Crawler/View/Events/{action}Event.gd");
                script.New(this, ev2, roles);
                // ev.Free() // resource counted!
                // if event wants to persist, then it creates a node and puts itself in there.
            }
            GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + ev2["timestamp"];

            if (!impatientMode && (int)ev2["subject"] == -1 && (string)ev2["action"] == "Wait")
            {
                if (AnyActorAnimating()) { break; }
            }
            eventQueue.RemoveAt(0);
            if (!impatientMode && (int)ev2["subject"] == -1 && (string)ev2["action"] == "SmallWait")
            {
                break;
            }

            // old code to replace.
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
            // End old code.
        }
        GetNode<RichTextLabel>("UILayer/DebugQueue").Text = "";
        for (int i = 0; i < eventQueue.Count && i < 30; i++)
        {
            Dictionary ev = eventQueue[i];
            // interpolated strings with quotes makes me uncomfortable.
            if ((string)ev["action"] == "Wait")
            {
                GetNode<RichTextLabel>("UILayer/DebugQueue").AppendBbcode($"[color=#AAAAFF]{i}\t{ev["subject"]}\t{ev["action"]}\n[/color]");
            }
            else
            {            
                GetNode<RichTextLabel>("UILayer/DebugQueue").AppendBbcode($"{i}\t{ev["subject"]}\t{ev["action"]}\n");
            }
        }
    }

    private void ModelSync()
    {
        // Sync things with model.
        // GD.Print("Sync!");

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
