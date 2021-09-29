using Godot;
using System;
using System.Collections.Generic;

public class RunAction : Action
{
    int limit;

    public RunAction(int limit = 5)
    {
        this.limit = limit;
    }

    public override bool Do(Model model, Entity e)
    {
        // TODO: Do not run macro if dangerous!
        if (GotoAction.AnyEnemiesInSight(model, e))
        {
            // No op.
            model.CoolerApiEvent(-1, "Print", "Cancelling Move. (Saw Enemy!)");
            return true;
        }

        (int x, int y) targetPos = GetTargetPos(e.position);
        if (!model.CanWalkFromTo(e.position, targetPos))
        {
            return true; // do nothing.
        }

        (int x, int y) oldTarget = targetPos;

        (int x, int y) nextTarget = (
                (targetPos.x - e.position.x) + oldTarget.x,
                (targetPos.y - e.position.y) + oldTarget.y
            );

        this.limit--;
        if (this.limit > 0)
        {
            this.SetTarget(nextTarget);
            e.queuedAction = this;
        }

        model.CoolerApiEvent(-1, "SmallWait");
        // target would be the new one lol
        bool success = new MoveAction().SetTarget(oldTarget).Do(model, e);
        // api.ApiEvent(new ModelEvent(-1, "Wait")); // painfully slow. see GotoAction.
        return success;
    }

    public override bool IsValid(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);
        if (!model.CanWalkFromTo(e.position, targetPos))
        {
            return false;
        }
        return true;
    }
}
