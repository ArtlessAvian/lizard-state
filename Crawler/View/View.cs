using Godot;
using System;
using System.Collections.Generic;

public partial class View : Node2D
{
    public static (int x, int y) TILESIZE = (32, 24);

    public List<ModelEvent> eventQueue;
    List<Actor> roles;

    // convenience
    public Actor playerActor;
    // super buggy but convenient
    public bool impatientMode = false;

    View()
    {
        eventQueue = new List<ModelEvent>();
        roles = new List<Actor>();
    }

    public override void _Ready()
    {
        this.ClearQueue();
        playerActor = roles[0];
        GetNode<CrawlerCamera>("Camera2D").focus = playerActor;
    }

    public override void _Process(float delta)
    {
        this.ClearQueue();
    }

    private void ClearQueue()
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
            GetNode<RichTextLabel>("UILayer/DebugLog").AppendBbcode("\n * " + ev.subject + " " + ev.action + " " + ev.args + " " + ev.obj);
            GetNode<MessageLog>("UILayer/MessageLog").HandleModelEvent(ev, roles);
        }
    }

    private void HandleNonActorEvent(ModelEvent ev)
    {
        if (ev.action == "Create")
        {
            // This entity info should never be abused outside this section!!
            Entity entity = ev.args as Entity;
            Actor puppet = GD.Load<PackedScene>($"res://Crawler/View/Actors/{entity.species.ResourceName}.tscn").Instance() as Actor;
            roles.Add(puppet);
            puppet.SyncWithEntity(entity);
            GetNode("Actors").AddChild(puppet);
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
