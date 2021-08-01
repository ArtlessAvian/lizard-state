using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

// Holds a model and shows what's happening.
public partial class View : Node2D
{
    public static Vector2 TILESIZE = new Vector2(32, 24);

    public List<Dictionary> eventQueue = new List<Dictionary>();
    public List<Actor> roles = new List<Actor>();

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
                Node node = (Node)GD.Load<GDScript>($"res://Crawler/View/Events/{action}Event.gd").New(ev2, roles);
                node.QueueFree();
            }
            GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + ev2["timestamp"];

            // Old code, to replace.
            ModelEvent ev;
            ev.subject = ev2.Contains("subject") ? (int)ev2["subject"] : -1;
            ev.action = (string)ev2["action"];
            ev.args = ev2.Contains("args") ? ev2["args"] : null;
            ev.obj = ev2.Contains("object") ? (int)ev2["object"] : -1;

            if (!impatientMode && ev.subject == -1 && ev.action == "Wait")
            {
                if (AnyActorAnimating()) { break; }
            }
            eventQueue.RemoveAt(0);
            if (!impatientMode && ev.subject == -1 && ev.action == "SmallWait")
            {
                break;
            }

            HandleNonActorEvent(ev, ev2);
            // End old code.

            // Everything gets sent to the logs.
            GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode("\n * " + ev.subject + " " + ev.action + " " + ev.obj + " " + ev.args);
            GetNode<MessageLog>("UILayer/MessageLog").HandleModelEvent(ev, roles);
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

    private void HandleNonActorEvent(ModelEvent ev, Dictionary ev2)
    {
        if (ev.action == "Create")
        {
            Entity entity = ev.args as Entity;

            // Find the actor, else, get a generic actor and try to recolor it i guess
            // TODO: don't make a new directory every time.
            Actor actor;
            if (new Godot.Directory().FileExists($"res://Crawler/View/Actors/{entity.species.ResourceName}.tscn"))
            {
                actor = GD.Load<PackedScene>($"res://Crawler/View/Actors/{entity.species.ResourceName}.tscn").Instance() as Actor;
            }
            else
            {
                actor = GD.Load<PackedScene>($"res://Crawler/View/Actor.tscn").Instance() as Actor;
                actor.GetNode<AnimatedSprite>("AnimatedSprite").Frames =
                        GD.Load<SpriteFrames>($"res://Crawler/View/Assets/ActorAtlas/{entity.species.ResourceName}.tres");
            }
            
            roles.Add(actor);
            actor.Name = entity.id.ToString();
            actor.ActAs(entity);

            FindNode("Actors").AddChild(actor);

            // HACK: lmao
            if (entity.id == 0)
            {
                playerActor = actor;
                GetNode("Camera2D").Set("focus", actor);
            }
        }

        else if (ev.action == "SeeMap")
        {
            GetNode<MapView>("Map").AddVision(ev2);
        }

        else if (ev.action == "Exit" || (ev.action == "Downed" && ev.subject == 0))
        {
            // Temporary!
            GetNode<MessageLog>("UILayer/MessageLog").AnchorTop = 0;
            GetNode<MessageLog>("UILayer/MessageLog").MarginTop = 20;
            GetNode<ColorRect>("UILayer/MessageLog/Background").Color = Color.FromHsv(0, 0, 0);
        }

        // else if (ev.action == "Print")
        // {
        //     string message = (string)ev.args;
        //     GD.Print(message);
        //     GetNode<RichTextLabel>("UILayer/MessageLog").AppendBbcode("\n * " + message);
        // }
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
