using System;
using System.Collections.Generic;
using Priority_Queue;

static class PathFinding
{
    public static (int steps, (int x, int y) nextStep) ShortestPathTo(IEnumerable<(int x, int y)> sources, (int x, int y) goal, Predicate<((int x, int y) from, (int x, int y) to)> Walkable)
    {
        Dictionary<(int, int), int> cost = new Dictionary<(int, int), int>();
        SimplePriorityQueue<(int, int)> frontier = new SimplePriorityQueue<(int, int)>();

        // Imagine one source connected to all these sources with 0 distance.
        // Therefore, this works.
        foreach((int x, int y) source in sources)
        {
            cost[source] = 0;
            frontier.Enqueue(source, GridHelper.Distance(source, goal));
        }

        while (frontier.Count > 0)
        {
            (int x, int y) current = frontier.Dequeue();
            // GD.Print("Currently " +  current);

            foreach ((int x, int y) neighbor in GridHelper.GetNeighbors(current))
            {
                // Filter neighbors. This search goes backwards, remember.
                // TODO: replace the thing with the thing
                if (!Walkable(((-1, -1), neighbor))) { continue; }

                // early exit
                if (neighbor.x == goal.x && neighbor.y == goal.y)
                {
                    return (cost[current] + 1, current);
                }

                if (cost[current] > 10) { continue; }

                if (!cost.ContainsKey(neighbor) || cost[current] + 1 < cost[neighbor])
                {
                    cost[neighbor] = cost[current] + 1;
                    frontier.EnqueueWithoutDuplicates(neighbor, cost[neighbor] + GridHelper.Distance(neighbor, goal));
                }
            }
        }

        // Fail!        
        return (Int32.MaxValue, goal);
    }
}