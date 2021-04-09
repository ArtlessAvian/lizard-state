using Godot;
using Priority_Queue;
using System;
using System.Collections.Generic;

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
        (int steps, (int, int) nextStep) = ShortestPathTo(api, enemyPositions, e.position);
        if (steps != Int32.MaxValue)
        {
            if (steps == 1) { return new AttackAction().Target(nextStep); }
            return new MoveAction().Target(nextStep);
        }

        // Move towards allies        
        (steps, nextStep) = ShortestPathTo(api, allyPositions, e.position);
        if (steps != Int32.MaxValue)
        {
            if (steps > 2) { return new MoveAction().Target(nextStep); }
        }

        return new MoveAction().Target(e.position);
    }

    public (int steps, (int x, int y) nextStep) ShortestPathTo(ModelAPI api, IEnumerable<(int x, int y)> sources, (int x, int y) goal)
    {
        Dictionary<(int, int), int> cost = new Dictionary<(int, int), int>();
        SimplePriorityQueue<(int, int)> frontier = new SimplePriorityQueue<(int, int)>();

        // Imagine one source connected to all these sources with 0 distance.
        // Therefore, this works.
        foreach((int x, int y) source in sources)
        {
            cost[source] = 0;
            frontier.Enqueue(source, Distance(source, goal));
        }

        while (frontier.Count > 0)
        {
            (int x, int y) current = frontier.Dequeue();
            // GD.Print("Currently " +  current);

            foreach ((int x, int y) neighbor in FilterTraversable(api, GetNeighbors(current)))
            {
                // early exit
                if (neighbor.x == goal.x && neighbor.y == goal.y)
                {
                    return (cost[current] + 1, current);
                }

                if (cost[current] > 10) { continue; }

                if (!cost.ContainsKey(neighbor) || cost[current] + 1 < cost[neighbor])
                {
                    cost[neighbor] = cost[current] + 1;
                    frontier.EnqueueWithoutDuplicates(neighbor, cost[neighbor] + Distance(neighbor, goal));
                }
            }
        }

        // Fail!        
        return (Int32.MaxValue, goal);
    }

    public IEnumerable<(int, int)> FilterTraversable(ModelAPI api, IEnumerable<(int, int)> stuff)
    {
        // Entity e = GetParent<Entity>();
        foreach ((int x, int y) thing in stuff)
        {
            if (!api.CanWalkFromTo((0, 0), thing)) { continue; }
            // Entity entityAt = api.GetEntityAt(thing.x, thing.y);
            // if (entityAt != null) { continue; }
            yield return thing;
        }
        yield break;
    }

    public static IEnumerable<(int, int)> GetNeighbors((int x, int y) a)
    {
        // lol lazy
        yield return (a.x - 1, a.y);
        yield return (a.x + 1, a.y);
        yield return (a.x, a.y - 1);
        yield return (a.x, a.y + 1);
        yield return (a.x - 1, a.y - 1);
        yield return (a.x + 1, a.y - 1);
        yield return (a.x - 1, a.y + 1);
        yield return (a.x + 1, a.y + 1);
    }

    public static int Distance((int x, int y) a, (int x, int y) b)
    {
        return Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
    }

    // public Dictionary SaveToDict()
    // {
    //     Dictionary dict = new Dictionary();
    //     return dict;
    // }
}