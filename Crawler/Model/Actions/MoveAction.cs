using Godot;
using System;
using System.Collections.Generic;

public class MoveAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e)) { return false; }

        AbsolutePosition stepPos = GridHelper.StepTowards(e.position, GetTargetPos(e.position), 1);

        if (stepPos.x == e.position.x && stepPos.y == e.position.y)
        {
            DoNothing(model, e);
            return true;
        }

        if (!model.CanWalkFromTo(e.position, stepPos))
        {
            GD.Print("You bump into the wall.");
            DoNothing(model, e);
            return true;
        }

        Entity entityAt = model.GetEntityAt(stepPos);
        if (!(entityAt is null))
        {
            if (entityAt.team != e.team)
            {
                GD.Print($"{e.species.displayName} bumps into {entityAt.species.displayName}");
                e.nextMove += 1;
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
        e.nextMove += 1;
        // model.CoolerApiEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));
    }

    private void DoMove(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);

        e.nextMove += (int)(1 * GridHelper.Distance(e.position, targetPos));
        e.position = targetPos;
        e.chargeStart = model.time;

        model.CoolerApiEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));
    }

    private void DoSwap(Model model, Entity e, Entity teammate)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);

        teammate.position = e.position;
        e.position = targetPos;
        e.nextMove += 1;

        model.CoolerApiEvent(e.id, "Swap", new Vector2(teammate.position.x, teammate.position.y), teammate.id);
    }

    public override bool IsValid(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);
        if (!TargetingType.CheckValid(e.position, targetPos, model.map.TileIsWall)) { return false; }
        return true;
    }

    public override IEnumerable<string> GetWarnings(Model model, Entity e)
    {
        if (!model.CanWalkFromTo(e.position, GetTargetPos(e.position))) { yield return "You can't walk there."; }
    }

    public override TargetingType.Type TargetingType => new TargetingType.Ray { range = 1, stopAtTarget = false };
}
