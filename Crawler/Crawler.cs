using Godot;
using System;
using System.Collections.Generic;

public class Crawler : Node2D
{
    public static (int x, int y) TILESIZE = (32, 24);

    // not saved
    List<ModelEvent> eventQueue;
    Dictionary<Entity, Actor> roles;

    // saved, of course.
    Model model;

    Crawler()
    {
        eventQueue = new List<ModelEvent>();
        roles = new Dictionary<Entity, Actor>();

        model = new Model(eventQueue);

        model.Tick(eventQueue);
    }

    // void Thing()
    // {
        // model.Tick(eventQueue);
    // }

    public override void _Process(float delta)
    {
        if (Input.IsKeyPressed((int)KeyList.Space))
        {
            model.Tick(eventQueue);
        }

        while (eventQueue.Count > 0)
        {
            ModelEvent ev = eventQueue[0];
            eventQueue.RemoveAt(0);

            if (ev.action == "Created")
            {
                Actor puppet = GD.Load<PackedScene>("res://Crawler/Actors/PlayerTegu.tscn").Instance() as Actor;
                roles.Add(ev.subject, puppet);
                puppet.SyncWithEntity(ev.subject);
                GetNode("Actors").AddChild(puppet);
            }

            GD.PrintS(ev.subject, ev.action, ev.args);

            roles[ev.subject].Perform(ev.action, ev.args);
        }
    }
}
