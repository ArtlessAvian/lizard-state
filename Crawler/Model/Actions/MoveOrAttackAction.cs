using Godot;
using System;
using System.Collections.Generic;

public class MoveOrAttackAction : Action
{
    public override bool Do(ModelAPI api, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        if (api.GetMap().GetCell(targetPos.x, targetPos.y) == 5)
        {
            return new ExitAction().SetTarget(targetPos).Do(api, e);
        }

        Entity targeted = api.GetEntityAt(targetPos);
        if (!(targeted is null) && targeted.team != e.team)
        {        
            return new AttackAction(e).SetTarget(targetPos).Do(api, e);
        }
        return new MoveAction().SetTarget(targetPos).Do(api, e);
    }

    public override bool IsValid(ModelAPI api, Entity e)
    {
        return true; // TODO: This one is tough. I could just make every action object and || them. (See MoveAction.cs too).
    }

    public override (int, int) Range => (1, 1);
}
