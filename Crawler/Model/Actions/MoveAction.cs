using Godot;
using System;
using System.Collections.Generic;

public class MoveAction : Action
{
    (int x, int y) displacement;

    // TODO: Get rid of random upcasting shenanigans
    public MoveAction(object args) 
    {
        displacement = ((int, int))args;
    }

    public bool Do(ModelAPI api, Entity e)
    {
        if (!api.CanWalkFromTo(0, 0, e.position.x + displacement.x, e.position.y + displacement.y))
        {
            return false;
        }
        Entity entityAt = api.GetEntityAt(e.position.x + displacement.x, e.position.y + displacement.y);
        if (!(entityAt is null) && entityAt != e)
        {
            if (e.team != entityAt.team)
            {
                GD.Print($"{e.species.displayName} bumps into the {entityAt.species.displayName}");
                return false;
            }
            else
            {
                entityAt.position.x = e.position.x;
                entityAt.position.y = e.position.y;

                e.position.x += displacement.x;
                e.position.y += displacement.y;
                e.nextMove += 10;

                api.NewEvent(new ModelEvent(-1, "Wait"));
                api.NewEvent(new ModelEvent(-1, "Print", $"{e.species.displayName} swaps with {entityAt.species.displayName}."));
                api.NewEvent(new ModelEvent(e.id, "Swap", entityAt.position, entityAt.id));
                api.NewEvent(new ModelEvent(-1, "Wait"));
                return true;
            }
        }
        // else
        e.position.x += displacement.x;
        e.position.y += displacement.y;
        e.nextMove += 10;

        api.NewEvent(new ModelEvent(e.id, "Move", e.position));
        return true;
    }
}
