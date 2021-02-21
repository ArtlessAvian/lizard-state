using Godot;
using System;
using System.Collections.Generic;

public class MoveAction : Action
{
    (int x, int y) displacement;

    public MoveAction((int x, int y) displacement)
    {
        this.displacement = displacement;
    }

    public bool Do(ModelAPI api, Entity e)
    {
        if (displacement.x == 0 && displacement.y == 0)
        {
            DoNothing(api, e);
            return true;
        }
        
        if (!api.CanWalkFromTo(0, 0, e.position.x + displacement.x, e.position.y + displacement.y))
        {
            return false;
        }

        Entity entityAt = api.GetEntityAt(e.position.x + displacement.x, e.position.y + displacement.y);
        if (!(entityAt is null))
        {
            if (entityAt.team != e.team)
            {
                GD.Print($"{e.species.displayName} bumps into {entityAt.species.displayName}");
                return false;
            }
            else
            {
                DoSwap(api, e, entityAt);
                return true;
            }
        }
        
        DoMove(api, e);
        return true;
    }

    private void DoNothing(ModelAPI api, Entity e)
    {
        e.nextMove += 10;
    }

    private void DoMove(ModelAPI api, Entity e)
    {
        e.position.x += displacement.x;
        e.position.y += displacement.y;
        e.nextMove += 10;

        // TODO: Maybe put elsewhere.
        e.dirtyVision |= e.providesVision;

        api.NewEvent(new ModelEvent(e.id, "Move", e.position));
    }

    private void DoSwap(ModelAPI api, Entity e, Entity teammate)
    {
        teammate.position.x = e.position.x;
        teammate.position.y = e.position.y;

        e.position.x += displacement.x;
        e.position.y += displacement.y;
        e.nextMove += 10;

        // TODO: Maybe put elsewhere.
        e.dirtyVision |= e.providesVision;
        teammate.dirtyVision |= teammate.providesVision;

        api.NewEvent(new ModelEvent(-1, "Wait"));
        // api.NewEvent(new ModelEvent(-1, "Print", $"{e.species.displayName} swaps with {teammate.species.displayName}."));
        api.NewEvent(new ModelEvent(e.id, "Swap", teammate.position, teammate.id));
        api.NewEvent(new ModelEvent(-1, "Wait"));
    }
}
