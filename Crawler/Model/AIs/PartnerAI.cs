using Godot;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
// using static PathFinding;

public class PartnerAI : AI
{
    public override Action GetMove(Model model, Entity e)
    {
        List<Entity> entities = model.GetEntitiesInRadius(e.position, 6);

        List<Entity> enemies = new List<Entity>();
        List<Entity> allies = new List<Entity>();
        foreach (Entity other in entities)
        {
            if (other.state == Entity.EntityState.UNALIVE) { continue; }
            if (other == e) { continue; }
            if (other.team == e.team)
            {
                allies.Add(other);
            }
            else
            {
                if (GridHelper.Distance(e.position, other.position) <= 4)
                {
                    enemies.Add(other);
                }
            }
        }

        // Attack the closest enemy.
        int closestDistance = 100;
        foreach (Entity enemy in enemies)
        {
            int distance = GridHelper.Distance(e.position, enemy.position);
            if (distance <= closestDistance)
            {
                closestDistance = distance;
            }
        }

        if (closestDistance <= 1 && e.species.rushAttack != null)
        {
            // Attack an enemy in melee range
            // TODO: randomly walk away.
            foreach (Entity enemy in enemies)
            {
                if (GridHelper.Distance(e.position, enemy.position) <= 1.5f)
                {
                    return e.species.rushAttack.SetTarget(enemy.position);
                }
            }
        }
        else
        {
            // get reach attacks in range.
            for (int i = 0; i < e.species.attacks.Count; i++)
            {
                Action attack = e.species.attacks[i];
                if (closestDistance <= attack.Range.max)
                {
                    foreach (Entity enemy in enemies)
                    {
                        if (GridHelper.Distance(e.position, enemy.position) == closestDistance)
                        {
                            return attack.SetTarget(enemy.position);
                        }
                    }
                }
            }

            // Select a move and try to attack an enemy in range.
            // AttackData bestAttack = e.species.bumpAttack;
            // float bestRangeMax = 1.5f;

            // foreach ((int, int) pos in enemyPositions)
            // {
            //     float distance = GridHelper.Distance(e.position, pos);
            //     if (distance <= bestRangeMax)
            //     {
            //         return new AttackAction().SetTarget(pos);
            //     }
            // }
        }



        // Move towards enemies.
        // Pathfinding should be cheap since the paths are short and probably straight lines.
        // IEnumerable<(int, int)> nextToEnemy = enemies.Select(enemy => GridHelper.GetNeighbors(enemy.position)).SelectMany(x => x);
        IEnumerable<(int, int)> enemyPositions = enemies.Select(enemy => enemy.position);
        PathFinder.PathResult result = PathFinder.ShortestPathToMany(e.position, enemyPositions, WalkableIgnoreTargets(model, enemies));
        if (result.success)
        {
            return new MoveAction().SetTarget(result.nextStep);
        }

        // Go towards lowest id ally. if no lowest, you are "leader".
        Entity leader = e;
        foreach (Entity ally in allies)
        {
            if (ally.id < leader.id)
            {
                leader = ally;
            }
        }

        if (leader != e)
        {
            result = PathFinder.ShortestPath(e.position, leader.position, WalkableIgnoreTarget(model, leader));
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
        else
        {
            return new MoveAction().SetTarget((e.position.x + (int)GD.Randi() % 3 - 1, e.position.y + (int)GD.Randi() % 3 - 1));
        }
    }

    // public Dictionary SaveToDict()
    // {
    //     Dictionary dict = new Dictionary();
    //     return dict;
    // }
}