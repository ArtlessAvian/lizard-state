using Godot;
using System;
using System.Collections.Generic;

public class MoveOrAttackAction : ActionTargeted
{
    public override bool Do(ModelAPI api, Entity e)
    {
        Entity targeted = api.GetEntityAt(target);
        if (!(targeted is null) && targeted.team != e.team)
        {        
            return new AttackAction().Target(target).Do(api, e);
        }
        return new MoveAction().Target(target).Do(api, e);
    }
}
