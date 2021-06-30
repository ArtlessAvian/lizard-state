using Godot;
using Priority_Queue;
using System;
using System.Collections.Generic;
// using static PathFinding;

public class AI
{
    public AI() {}

    public Action GetMove(ModelAPI api, Entity e)
    {
        List<Entity> entities = api.GetEntitiesInRadius(e.position, 3);
        
        List<(int, int)> enemyPositions = new List<(int, int)>();
        List<(int, int)> allyPositions = new List<(int, int)>();
        foreach (Entity other in entities)
        {
            if (other.downed) { continue; }
            if (other == e) { continue; }
            if (other.team == e.team)
            {
                allyPositions.Add(other.position);
            }
            else
            {
                enemyPositions.Add(other.position);
            }
        }

        // Attack enemies, or move towards them
        (int steps, (int, int) nextStep) = PathFinding.ShortestPathToMany(e.position, enemyPositions, Walkable(api));
        if (steps != Int32.MaxValue)
        {
            if (steps == 1) { return new AttackAction(e.species.bumpAttack).SetTarget(nextStep); }
            return new MoveAction().SetTarget(nextStep);
        }

        // Move towards allies        
        (steps, nextStep) = PathFinding.ShortestPathToMany(e.position, allyPositions, Walkable(api));
        if (steps != Int32.MaxValue)
        {
            if (steps > 2) { return new MoveAction().SetTarget(nextStep); }
        }

        return new MoveAction().SetTarget(e.position);
    }

    // kinda ugly but i dont care.
    private Predicate<((int x, int y) from, (int x, int y) to)> Walkable(ModelAPI api)
    {
        return (((int, int) from, (int, int) to) tuple) => api.CanWalkFromTo(tuple.from, tuple.to);
    }

    // public Dictionary SaveToDict()
    // {
    //     Dictionary dict = new Dictionary();
    //     return dict;
    // }
}