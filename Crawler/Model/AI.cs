using Godot;
using Priority_Queue;
using System;
using System.Collections.Generic;
// using static PathFinding;

public abstract class AI : Resource
{
    public abstract Action GetMove(Model model, Entity e);

    // kinda ugly but i dont care.
    protected static Predicate<((int x, int y) from, (int x, int y) to)> Walkable(Model model, Entity e)
    {
        return (((int, int) from, (int, int) to) tuple) => (
            model.CanWalkFromTo(tuple.from, tuple.to) &&
            (model.GetEntityAt(tuple.to) == null || model.GetEntityAt(tuple.to).team != e.team)
        );
    }

    protected static Predicate<((int x, int y) from, (int x, int y) to)> WalkThroughAllies(Model model)
    {
        return (((int, int) from, (int, int) to) tuple) => (
            model.CanWalkFromTo(tuple.from, tuple.to)
        );
    }
}