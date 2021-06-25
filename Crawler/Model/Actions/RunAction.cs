using Godot;
using System;
using System.Collections.Generic;

public class RunAction : ActionTargeted
{
    int limit;

    public RunAction(int limit = 5)
    {
        this.limit = limit;
    }

    public override bool Do(ModelAPI api, Entity e)
    {
        if (api.CanWalkFromTo(e.position, target))
        {
            (int x, int y) oldTarget = target;

            (int x, int y) nextTarget = (
                    (target.x - e.position.x) + oldTarget.x,
                    (target.y - e.position.y) + oldTarget.y
                );
            
            if (this.limit >= 0)
            {
                this.limit--;
                this.Target(nextTarget);
                e.queuedAction = this; 
            }

            // target would be the new one lol
            return new MoveAction().Target(oldTarget).Do(api, e);
        }
        return false;
    }
}
