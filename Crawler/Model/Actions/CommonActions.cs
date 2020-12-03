using Godot;
using System;
using System.Collections.Generic;

public class MoveAction : Action
{
    (int x, int y) displacement;

    public MoveAction(object args) 
    {
        displacement = ((int, int))args;
    }

    public bool Do(List<ModelEvent> eventQueue, Entity e)
    {
        e.position.x += displacement.x;
        e.position.y += displacement.y;
        e.nextMove += 1;

        ModelEvent ev;
        ev.subject = e;
        ev.action = "Moved";
        ev.args = (e.position.x, e.position.y);
        eventQueue.Add(ev);

        return true;
    }
}
