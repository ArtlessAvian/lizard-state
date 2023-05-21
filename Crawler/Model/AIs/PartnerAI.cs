using Godot;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
// using static PathFinding;

public class PartnerAI : AI
{
    public override IEnumerable<(Action, bool)> GetMoves(Model model, Entity e)
    {
        // TODO: GetEntitiesInLOS (but also entities in team).
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

        if (closestDistance <= 1 && e.species.bumpAttack != null)
        {
            // Attack an enemy in melee range
            // TODO: randomly walk away.
            foreach (Entity enemy in enemies)
            {
                yield return (e.species.bumpAttack.SetTarget(enemy.position), false);
            }
        }
        else
        {
            // get reach attacks in range.
            for (int i = 0; i < e.species.abilities.Count; i++)
            {
                Action attack = e.species.abilities[i];
                // TODO: if ability is not attack, continue;
                foreach (Entity enemy in enemies)
                {
                    yield return (attack.SetTarget(enemy.position), false);
                }
            }
        }



        // Move towards enemies.
        // Pathfinding should be cheap since the paths are short and probably straight lines.
        // IEnumerable<(int, int)> nextToEnemy = enemies.Select(enemy => GridHelper.GetNeighbors(enemy.position)).SelectMany(x => x);
        IEnumerable<AbsolutePosition> enemyPositions = enemies.Select(enemy => enemy.position);
        PathFinder.PathResult result = PathFinder.ShortestPathToMany(e.position, enemyPositions, WalkableIgnoreTargets(model, enemies));
        if (result.success)
        {
            yield return (new MoveAction().SetTarget(result.nextStep), false);
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
            if (result.success && result.steps > 2.5f)
            {
                yield return (new MoveAction().SetTarget(result.nextStep), false);
            }
            yield return (new MoveAction().SetTargetRelative((0, 0)), true);
        }
        else
        {
            yield return (new MoveAction().SetTargetRelative(((int)(GD.Randi() % 3) - 1, (int)(GD.Randi() % 3) - 1)), false);
            yield return (new MoveAction().SetTargetRelative((0, 0)), true);
        }
    }

    // public Dictionary SaveToDict()
    // {
    //     Dictionary dict = new Dictionary();
    //     return dict;
    // }
}