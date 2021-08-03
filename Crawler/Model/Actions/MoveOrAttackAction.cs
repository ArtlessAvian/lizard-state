using Godot;
using System;
using System.Collections.Generic;

public class MoveOrAttackAction : Action
{
    public override bool Do(ModelAPI api, Entity e)
    {
        return GetMacroedAction(api, e).Do(api, e);
    }

    public override bool IsValid(ModelAPI api, Entity e)
    {
        return GetMacroedAction(api, e).IsValid(api, e);
        // return GridHelper.Distance(e.position, GetTargetPos(e.position)) <= 1.5f;
        // return true; // TODO: This one is tough. I could just make every action object and || them. (See MoveAction.cs too).
    }

    // only makes sense for abilities that might be aimed.
    // public override (float, float) Range => (1, 10);

    public Action GetMacroedAction(ModelAPI api, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        if (api.GetMap().GetCell(targetPos.x, targetPos.y) == 5)
        {
            return new ExitAction().SetTarget(targetPos);
        }

        Entity targeted = api.GetEntityAt(targetPos);
        if (!(targeted is null) && targeted.team != e.team)
        {        
            return new AttackAction(e).SetTarget(targetPos);
        }
        return new MoveAction().SetTarget(targetPos);
    }
}
