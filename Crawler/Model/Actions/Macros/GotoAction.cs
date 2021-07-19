using Godot;
using System;
using System.Collections.Generic;

public class GotoAction : Action
{
    PathFinder.PathResult result;

    public override bool Do(ModelAPI api, Entity e)
    {
        // TODO: Do not run macro if dangerous!

        (int x, int y) targetPos = GetTargetPos(e.position);

        // TODO: uhh, what does this condition do?
        if (api.CanWalkFromTo(e.position, targetPos))
        {
            if (result == null)
            {
                PathFinder pather = new PathFinder();
                pather.maxLength = 100000;
                result = PathFinder.ShortestPath(e.position, targetPos, Walkable(api));
                if (result.steps == int.MaxValue)
                {
                    return false;
                }
            } 
            
            bool success = new MoveAction().SetTarget(result.nextStepFor[e.position]).Do(api, e);

            if (!success) { return false; }
            
            // queue same action object
            if (e.position.x != targetPos.x || e.position.y != targetPos.y)
            {
                e.queuedAction = this;
            }

            // api.ApiEvent(new ModelEvent(-1, "Wait")); // painfully slow. also, TODO: make systems more like callbacks??
            return success;
        }
        return false;
    }

    public override bool IsValid(ModelAPI api, Entity e)
    {
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
