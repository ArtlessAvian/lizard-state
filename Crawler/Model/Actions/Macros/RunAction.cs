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
        // TODO: Do not run macro if dangerous!

        (int x, int y) targetPos = GetTargetPos(e.position);
        if (api.CanWalkFromTo(e.position, targetPos))
        {
            (int x, int y) oldTarget = targetPos;

            (int x, int y) nextTarget = (
                    (targetPos.x - e.position.x) + oldTarget.x,
                    (targetPos.y - e.position.y) + oldTarget.y
                );
            
            if (this.limit >= 0)
            {
                this.limit--;
                this.SetTarget(nextTarget);
                e.queuedAction = this; 
            }

            // target would be the new one lol
            bool success = new MoveAction().SetTarget(oldTarget).Do(api, e);
            // api.ApiEvent(new ModelEvent(-1, "Wait")); // painfully slow. see GotoAction.
            return success;
        }
        return false;
    }
}
