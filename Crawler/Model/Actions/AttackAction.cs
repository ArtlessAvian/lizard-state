using Godot;
using System.Collections.Generic;

public class AttackAction : Action
{
    (int x, int y) direction;

    public AttackAction(object args) 
    {
        direction = ((int, int))args;
    }

    public bool Do(ModelAPI api, Entity e)
    {
        Entity target = api.GetEntityAt(e.position.x + direction.x, e.position.y + direction.y);

        if ((target is null) || target == e)
        {
            return false;
        }

        api.NewEvent(new ModelEvent(-1, "Wait"));

        e.nextMove += 10;

        api.NewEvent(new ModelEvent(-1, "Wait"));
        return true;
    }
}
