using Godot;
using System;
using System.Collections.Generic;

public class MoveOrAttackAction : ActionTargeted
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
            return new AttackAction().SetTarget(targetPos).Do(api, e);
        }
        return new MoveAction().SetTarget(targetPos).Do(api, e);
    }
}
