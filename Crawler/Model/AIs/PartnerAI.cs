using Godot;
using Priority_Queue;
using System;
using System.Collections.Generic;
// using static PathFinding;

public class PartnerAI : AI
{
    public override Action GetMove(Model model, Entity e)
    {
        List<Entity> entities = model.GetEntitiesInRadius(e.position, 6);
        
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
                if (GridHelper.Distance(e.position, other.position) <= 4)
                {
                    enemyPositions.Add(other.position);
                }
            }
        }

        // Select a move and try to attack an enemy in range.
        int bestAttack = -1;
        float bestRangeMax = 1.5f;
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
            float distance = GridHelper.Distance(e.position, pos);
            if (distance <= bestRangeMax)
            {
                return new AttackAction(e, bestAttack).SetTarget(pos);
            }
        }

        // Move towards enemies.
        // Pathfinding should be cheap since the paths are short and probably straight lines.
        // PathFinder.PathResult result = PathFinder.ShortestPathToMany(e.position, enemyPositions, Walkable(api));
        PathFinder.PathResult result = PathFinder.ShortestPathToMany(e.position, enemyPositions, Walkable(model, e));
        if (result.success)
        {
            GD.Print(e.id, " moving towards thing ", bestRangeMax);
            return new MoveAction().SetTarget(result.nextStep);
        }

        // low priority todo: big clumps of ai. 
        result = PathFinder.ShortestPathToMany(e.position, allyPositions, WalkThroughAllies(model));
        if (result.success)
        {
            if (result.steps > 2.5f)
            {
                return new MoveAction().SetTarget(result.nextStep);
            }
            return new MoveAction().SetTarget(e.position);
        }

        return new MoveAction().SetTarget(e.position);
    }
    
    // public Dictionary SaveToDict()
    // {
    //     Dictionary dict = new Dictionary();
    //     return dict;
    // }
}