using Godot;
using System;
using System.Collections.Generic;

public class MoveAction : Action
{
    public override bool Do(ModelAPI api, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        if (targetPos.x == e.position.x && targetPos.y == e.position.y)
        {
            DoNothing(api, e);
            return true;
        }
        
        if (!api.CanWalkFromTo(e.position, targetPos))
        {
            return false;
        }

        Entity entityAt = api.GetEntityAt(targetPos);
        if (!(entityAt is null))
        {
            if (entityAt.team != e.team)
            {
                GD.Print($"{e.species.displayName} bumps into {entityAt.species.displayName}");
                e.nextMove += 10;
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
        (int x, int y) targetPos = GetTargetPos(e.position);

        e.nextMove += (int)(10 * GridHelper.Distance(e.position, targetPos));
        e.position = targetPos;

        // TODO: Maybe put elsewhere.
        e.dirtyVision |= e.providesVision;

        api.CoolerApiEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));
    }

    private void DoSwap(ModelAPI api, Entity e, Entity teammate)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        teammate.position = e.position;
        e.position = targetPos;
        e.nextMove += 10;

        // TODO: Maybe put elsewhere.
        e.dirtyVision |= e.providesVision;
        teammate.dirtyVision |= teammate.providesVision;

        api.CoolerApiEvent(-1, "Wait");
        // api.NewEvent(new ModelEvent(-1, "Print", $"{e.species.displayName} swaps with {teammate.species.displayName}."));
        api.CoolerApiEvent(e.id, "Swap", new Vector2(teammate.position.x, teammate.position.y), teammate.id);
        api.CoolerApiEvent(-1, "Wait");
    }

    public override bool IsValid(ModelAPI api, Entity e)
    {
        // TODO: This one is tough. Usually true. (See MoveOrAttackAction.cs too).
        return GridHelper.Distance(e.position, GetTargetPos(e.position)) <= 1.5f;
    }

    public override (float, float) Range => (1, 1.5f);
}
