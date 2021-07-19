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
        List<Entity> entities = api.GetEntitiesInRadius(e.position, 4);
        
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

        // Select a move and try to attack an enemy in range.
        int bestAttack = -1;
        int bestRangeMax = 1;
        for (int i = 0; i < e.species.attacks.Count; i++)
        {
            AttackData data = e.species.attacks[i];
            if (data.energy < e.energy)
            {
                bestAttack = i;
                bestRangeMax = data.range;
                break;
            }
        }
        foreach ((int, int) pos in enemyPositions)
        {
            int distance = GridHelper.Distance(e.position, pos);
            if (distance <= bestRangeMax)
            {
                return new AttackAction(e, bestAttack).SetTarget(pos);
            }
        }

        // Move towards enemies, or bump them.
        // Pathfinding should be cheap since the paths are short and probably straight lines.
        PathFinder.PathResult result = PathFinder.ShortestPathToMany(e.position, enemyPositions, Walkable(api));
        if (result.steps != Int32.MaxValue)
        {
            return new MoveAction().SetTarget(result.nextStep);
        }

        // if no enemies, Move towards allies        
        result = PathFinder.ShortestPathToMany(e.position, allyPositions, Walkable(api));
        if (result.steps != Int32.MaxValue)
        {
            if (result.steps > 2) { return new MoveAction().SetTarget(result.nextStep); }
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