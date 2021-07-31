using Godot;
using System;
using System.Collections.Generic;

public class GotoAction : Action
{
    PathFinder.PathResult result;

    public override bool Do(ModelAPI api, Entity e)
    {
        if (AnyEnemiesInSight(api, e))
        {
            // No op! On view, print stuff.
            api.CoolerApiEvent(-1, "Print", "Cancelling Move. (Saw Enemy!)");
            return true;
        }

        (int x, int y) targetPos = GetTargetPos(e.position);

        FindPathLazy(api, e.position, targetPos);
        
        // api.CoolerApiEvent(-1, "Wait"); // painfully slow. make optional?
        bool success = new MoveAction().SetTarget(result.nextStepFor[e.position]).Do(api, e);

        if (!success) { return false; }
        
        // queue same action object
        if (e.position.x != targetPos.x || e.position.y != targetPos.y)
        {
            e.queuedAction = this;
        }

        return success;
    }

    private bool FindPathLazy(ModelAPI api, (int, int) from, (int, int) to)
    {
        if (result == null)
        {
            PathFinder pather = new PathFinder();
            pather.maxLength = 100000;
            result = PathFinder.ShortestPath(from, to, Walkable(api));
        }
        return result.steps != int.MaxValue;
    }

    private bool AnyEnemiesInSight(ModelAPI api, Entity e)
    {
        foreach (Entity other in api.GetEntitiesInSight(0))
        {
            if (other.team != 0 && !other.downed)
            {
                return true;
            }
        }
        return false;
    }

    public override bool IsValid(ModelAPI api, Entity e)
    {
        // assume path does not change after this is called.
        if (!FindPathLazy(api, e.position, GetTargetPos(e.position)))
        {
            return false;
        }

        return true;
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
