using Godot;
using System;
using System.Collections.Generic;

// Holds a model and shows what's happening.
public partial class View : Node2D
{
    public static (int x, int y) TILESIZE = (32, 24);

    public List<ModelEvent> eventQueue = new List<ModelEvent>();
    public List<Actor> roles = new List<Actor>();

    // convenience
    [Export] public Actor playerActor;
    // super buggy but convenient
    [Export] public bool impatientMode = false;

    private bool queueSync = false;

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

        if (queueSync && !this.AnyActorAnimating())
        {
            queueSync = false;
            this.ModelSync();
            Model debugggModel = GetNode<Model>("../Model");
            GetNode<RichTextLabel>("UILayer/Time").BbcodeText = "Debug Time: " + debugggModel.time;
        }
    }

    public void ClearQueue()
    {
        while (eventQueue.Count > 0)
        {
            ModelEvent ev = eventQueue[0];
            if (ev.subject == -1 && ev.action == "Wait")
            {
                if (!impatientMode && AnyActorAnimating()) { break; }
            }
            eventQueue.RemoveAt(0);

            HandleNonActorEvent(ev);

            if (ev.subject >= 0)
            {
                roles[ev.subject].PerformAsSubject(ev, roles);
                if (ev.obj >= 0) { roles[ev.obj].PerformAsObject(ev, roles); }
            }

            // Everything gets sent to the logs.
            GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode("\n * " + ev.subject + " " + ev.action + " " + ev.obj + " " + ev.args);
            GetNode<MessageLog>("UILayer/MessageLog").HandleModelEvent(ev, roles);
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

    private void HandleNonActorEvent(ModelEvent ev)
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
                        // TODO: Put ActorData in Assets or something.
            }
            
            roles.Add(actor);
            actor.Name = entity.id.ToString();
            actor.ActAs(entity);

            FindNode("Actors").AddChild(actor);

            // HACK: lmao
            if (entity.id == 0)
            {
                playerActor = actor;
                GetNode<CrawlerCamera>("Camera2D").focus = actor;
            }
        }

        else if (ev.action == "SeeMap")
        {
            ((int x, int y) center, int[,] tiles) = (((int, int), int[,]))ev.args;
            GetNode<MapView>("Map").AddVision(ev.subject, center, tiles);
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
