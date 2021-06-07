using Godot;
using System;
using System.Collections.Generic;

public partial class View : Node2D
{
    public static (int x, int y) TILESIZE = (32, 24);

    public Model model = new Model();

    public List<ModelEvent> eventQueue;
    public List<Actor> roles = new List<Actor>();

    // convenience
    public Actor playerActor;
    // super buggy but convenient
    public bool impatientMode = true;

    bool queueSync = false;

    public override void _Ready()
    {
        this.eventQueue = model.eventQueue;
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
        GD.Print("Sync!", model.time);

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
            Actor actor = GD.Load<PackedScene>($"res://Crawler/View/Actor.tscn").Instance() as Actor;

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
