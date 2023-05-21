using Godot;
using System;
using System.Collections.Generic;

public class GotoAction : Action
{
    PathFinder.PathResult result;
    bool stepped = false;

    public override bool Do(Model model, Entity e)
    {
        if (stepped && AnyEnemiesInSight(model, e))
        {
            // No op! On view, print stuff.
            model.CoolerApiEvent(-1, "Print", "Cancelling Move. (Saw Enemy!)");
            return true;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);

        FindPathLazy(model, e.position, targetPos);

        // model.CoolerApiEvent(-1, "SmallWait");
        bool success = new MoveAction().SetTarget(result.nextStepFor[e.position]).Do(model, e);

        if (!success) { return false; }

        // queue same action object
        if (!(e.position.x == targetPos.x && e.position.y == targetPos.y))
        {
            e.queuedAction = this;
            stepped = true;
        }

        return success;
    }

    private bool FindPathLazy(Model model, AbsolutePosition from, AbsolutePosition to)
    {
        if (result == null)
        {
            PathFinder pather = new PathFinder();
            pather.maxLength = 100000;
            pather.source = from;
            pather.goals = new List<AbsolutePosition>() { to };
            pather.walkable = Walkable(model);
            result = pather.Run();
        }
        return result.success;
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

    private Predicate<(AbsolutePosition from, AbsolutePosition to)> Walkable(Model model)
    {
        FogOfWarSystem fog = model.GetSystem<FogOfWarSystem>();

        return ((AbsolutePosition from, AbsolutePosition to) tuple) =>
                model.CanWalkFromTo(tuple.from, tuple.to) &&
                fog.GetCell(tuple.from.x, tuple.from.y) != -1 &&
                fog.GetCell(tuple.to.x, tuple.to.y) != -1;
    }
}
