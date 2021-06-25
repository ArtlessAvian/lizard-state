using Godot;
using System;
using System.Collections.Generic;

public class RunAction : ActionTargeted
{
    public override bool Do(ModelAPI api, Entity e)
    {
        if (api.CanWalkFromTo(e.position, target))
        {
            (int x, int y) nextTarget = (
                    (target.x - e.position.x) + target.x,
                    (target.y - e.position.y) + target.y
                );
            e.queuedAction = new RunAction().Target(nextTarget);
            return new MoveAction().Target(target).Do(api, e);
        }
        return false;
    }
}
