using Godot;
using System;
using System.Collections.Generic;

public class GotoAction : ActionTargeted
{
    public override bool Do(ModelAPI api, Entity e)
    {
        // TODO: Do not run macro if dangerous!

        (int x, int y) targetPos = GetTargetPos(e.position);

        if (api.CanWalkFromTo(e.position, targetPos))
        {
            // pathfind to target
            // TODO: Save result, recalculate on fail
            (int steps, (int x, int y) nextStep) p =
                    PathFinding.ShortestPathTo(e.position, targetPos, Walkable(api));
            
            if (p.steps == Int32.MaxValue)
            {
                return false;
            }

            bool success = new MoveAction().SetTarget(p.nextStep).Do(api, e);

            if (!success) { return false; }
            
            // queue same action object
            if (p.nextStep.x != targetPos.x || p.nextStep.y != targetPos.y)
            {
                e.queuedAction = this;
            }

            // api.ApiEvent(new ModelEvent(-1, "Wait")); // painfully slow. also, TODO: make systems more like callbacks??
            return success;
        }
        return false;
    }

    private Predicate<((int x, int y) from, (int x, int y) to)> Walkable(ModelAPI api)
    {
        Model model = (Model)api; // whatever.
        VisionSystem fog = model.GetNode<VisionSystem>("Systems/Vision");

        return (((int x, int y) from, (int x, int y) to) tuple) =>
                api.CanWalkFromTo(tuple.from, tuple.to) &&
                fog.GetCell(tuple.from.x, tuple.from.y) != -1 &&
                fog.GetCell(tuple.to.x, tuple.to.y) != -1;
    }
}
