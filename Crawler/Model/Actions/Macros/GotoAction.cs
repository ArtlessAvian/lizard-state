using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class GotoAction : Action
{
    PathFinder.PathResult pathResult = null;

    public override Dictionary Do(Model model, Entity e)
    {
        // bool enemiesBeforeMove = AnyEnemiesInSight(model, e);
        // if (AnyEnemiesInSight(model, e))
        // {
        //     // No op! On view, print stuff.
        //     model.CoolerApiEvent(-1, "Print", "Cancelling Move. (Saw Enemy!)");
        //     return true;
        // }

        (int x, int y) targetPos = GetTargetPos(e.position);

        FindPathLazy(model, e.position, targetPos);

        Dictionary result = new MoveAction().SetTarget(pathResult.nextStepFor[e.position]).Do(model, e);

        if (result is null) { return null; }

        // queue same action object
        if (!(AnyEnemiesInSight(model, e) || e.position.x == targetPos.x && e.position.y == targetPos.y))
        {
            e.queuedAction = this;
        }

        return result;
    }

    private bool FindPathLazy(Model model, (int, int) from, (int, int) to)
    {
        if (pathResult == null)
        {
            PathFinder pather = new PathFinder();
            pather.maxLength = 100000;
            pather.source = from;
            pather.goals = new List<(int, int)>() { to };
            pather.walkable = Walkable(model);
            pathResult = pather.Run();
        }
        return pathResult.success;
    }

    // shared by RunAction.
    public static bool AnyEnemiesInSight(Model model, Entity e)
    {
        foreach (Entity other in model.GetEntitiesInSight(0))
        {
            if (other.team != 0 && other.state != Entity.EntityState.UNALIVE)
            {
                return true;
            }
        }
        return false;
    }

    public override bool IsValid(Model model, Entity e)
    {
        // assume path does not change after this is called.
        if (!FindPathLazy(model, e.position, GetTargetPos(e.position)))
        {
            return false;
        }

        return true;
    }

    private Predicate<((int x, int y) from, (int x, int y) to)> Walkable(Model model)
    {
        FogOfWarSystem fog = model.GetNode<FogOfWarSystem>("Systems/Fog");

        return (((int x, int y) from, (int x, int y) to) tuple) =>
                model.CanWalkFromTo(tuple.from, tuple.to) &&
                fog.GetCell(tuple.from.x, tuple.from.y) != -1 &&
                fog.GetCell(tuple.to.x, tuple.to.y) != -1;
    }
}
