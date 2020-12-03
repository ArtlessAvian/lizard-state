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
    bool notPlayerTurn = false;

    Crawler()
    {
        eventQueue = new List<ModelEvent>();
        roles = new Dictionary<Entity, Actor>();

        model = new Model(eventQueue);
    }

    public override void _Process(float delta)
    {
        // MOVE ME
        if (Input.IsActionJustPressed("move_up"))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((0, -1)));
            notPlayerTurn = true;
        }
        if (Input.IsActionJustPressed("move_down"))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((0, 1)));
            notPlayerTurn = true;
        }
        if (Input.IsActionJustPressed("move_left"))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((-1, 0)));
            notPlayerTurn = true;
        }
        if (Input.IsActionJustPressed("move_right"))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((1, 0)));
            notPlayerTurn = true;
        }

        while (notPlayerTurn) // and not timed out
        {
            if (!model.DoEntityAction(eventQueue))
            {
                // let the player move again.
                notPlayerTurn = false;
                break;
            }
        }

        while (eventQueue.Count > 0)
        {
            ModelEvent ev = eventQueue[0];
            eventQueue.RemoveAt(0);

            if (ev.action == "Created")
            {
                Actor puppet = GD.Load<PackedScene>($"res://Crawler/UI/Actors/{ev.subject.species.ResourceName}.tscn").Instance() as Actor;
                roles.Add(ev.subject, puppet);
                puppet.SyncWithEntity(ev.subject);
                GetNode("Actors").AddChild(puppet);
            }

            GD.PrintS(ev.subject, ev.action, ev.args);

            roles[ev.subject].Perform(ev.action, ev.args);
        }
    }
}
