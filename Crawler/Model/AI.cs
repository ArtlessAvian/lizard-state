using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class AI : Resource
{
    public abstract IEnumerable<(Action, bool ignoreWarning)> GetMoves(Model model, Entity e);

    // kinda ugly but i dont care.
    protected static Predicate<((int x, int y) from, (int x, int y) to)> Walkable(Model model)
    {
        return (((int, int) from, (int, int) to) tuple) => (
            model.CanWalkFromTo(tuple.from, tuple.to) &&
            (model.GetEntityAt(tuple.to) == null)
        );
    }

    protected static Predicate<((int x, int y) from, (int x, int y) to)> WalkableIgnoreTarget(Model model, Entity target)
    {
        return (((int, int) from, (int, int) to) tuple) => (
            model.CanWalkFromTo(tuple.from, tuple.to) &&
            (model.GetEntityAt(tuple.to) == null || target == model.GetEntityAt(tuple.to))
        );
    }

    protected static Predicate<((int x, int y) from, (int x, int y) to)> WalkableIgnoreTargets(Model model, IEnumerable<Entity> targets)
    {
        return (((int, int) from, (int, int) to) tuple) => (
            model.CanWalkFromTo(tuple.from, tuple.to) &&
            (model.GetEntityAt(tuple.to) == null || targets.Contains(model.GetEntityAt(tuple.to)))
        );
    }
}