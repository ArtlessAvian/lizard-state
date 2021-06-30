using Godot;
using System;
using System.Collections.Generic;

public class GotoAction : ActionTargeted
{
    public override bool Do(ModelAPI api, Entity e)
    {
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

            // queue same action object
            if (p.nextStep.x != targetPos.x || p.nextStep.y != targetPos.y)
            {
                e.queuedAction = this;
            }
            return new MoveAction().SetTarget(p.nextStep).Do(api, e);
        }
        return false;
    }

    private Predicate<((int x, int y) from, (int x, int y) to)> Walkable(ModelAPI api)
    {
        return (((int x, int y) from, (int x, int y) to) tuple) =>
                api.CanWalkFromTo(tuple.from, tuple.to) &&
                api.GetMap().fog.GetCell(tuple.from.x, tuple.from.y) != -1 &&
                api.GetMap().fog.GetCell(tuple.to.x, tuple.to.y) != -1;
    }
}
