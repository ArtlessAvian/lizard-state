using Godot;
using System;
using System.Collections.Generic;

public class MoveAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        // GD.Print("test");

        if (!IsValid(model, e)) { return false; }

        (int x, int y) targetPos = GetTargetPos(e.position);

        if (targetPos.x == e.position.x && targetPos.y == e.position.y)
        {
            DoNothing(model, e);
            return true;
        }

        Entity entityAt = model.GetEntityAt(targetPos);
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
                DoSwap(model, e, entityAt);
                return true;
            }
        }
        
        DoMove(model, e);
        return true;
    }

    private void DoNothing(Model model, Entity e)
    {
        e.nextMove += 10;
        // model.CoolerApiEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));
    }

    private void DoMove(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        e.nextMove += (int)(10 * GridHelper.Distance(e.position, targetPos));
        e.position = targetPos;

        // TODO: Maybe put elsewhere.
        e.dirtyVision |= e.providesVision;

        model.CoolerApiEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));
    }

    private void DoSwap(Model model, Entity e, Entity teammate)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        teammate.position = e.position;
        e.position = targetPos;
        e.nextMove += 10;

        // TODO: Maybe put elsewhere.
        e.dirtyVision |= e.providesVision;
        teammate.dirtyVision |= teammate.providesVision;

        model.CoolerApiEvent(-1, "Wait");
        // api.NewEvent(new ModelEvent(-1, "Print", $"{e.species.displayName} swaps with {teammate.species.displayName}."));
        model.CoolerApiEvent(e.id, "Swap", new Vector2(teammate.position.x, teammate.position.y), teammate.id);
        model.CoolerApiEvent(-1, "Wait");
    }

    public override bool IsValid(Model model, Entity e)
    {
        // TODO: This one is tough. Usually true. (See MoveOrAttackAction.cs too).

        (int x, int y) targetPos = GetTargetPos(e.position);
        if (GridHelper.Distance(e.position, GetTargetPos(e.position)) > 1.5f) { return false; }
        if (!model.CanWalkFromTo(e.position, targetPos)) { return false; }
        return true;
    }

    public override (float, float) Range => (1, 1.5f);
}
