using System;
using System.Collections.Generic;
using Priority_Queue;

public class PathFinder
{
    // Parameters before a run.
    public (int x, int y) source;
    public IEnumerable<(int x, int y)> goals;
    public Predicate<((int x, int y) from, (int x, int y) to)> walkable;

    // Optional.
    public int maxLength = 18;
    // worst case default is <1000 tiles per goal searched. 2\sqrt{2} r^2

    // Results, ofc.
    public class PathResult
    {
        public bool success = false;
        public float steps = -1;
        public (int, int) nextStep; // Convenience. nextStepFor[source]
        public Dictionary<(int, int), (int, int)> nextStepFor;
    }
    PathResult result = new PathResult();

    /// <summary>
    /// Finds the shortest path from one source to many goals.
    /// Returns results as soon as closest goal is found.
    /// </summary>
    /// Actually, it searches from the many goals to the source ;)
    /// Makes the heuristic easy.
    public PathResult Run()
    {
        Dictionary<(int, int), int> cost = new Dictionary<(int, int), int>();
        SimplePriorityQueue<(int, int)> frontier = new SimplePriorityQueue<(int, int)>();

        result.nextStepFor = new Dictionary<(int, int), (int, int)>();

        // Imagine one source connected to all these sources with 0 distance.
        // Therefore, this works.
        foreach ((int x, int y) goal in goals)
        {
            cost[goal] = 0;
            frontier.Enqueue(goal, GridHelper.Distance(goal, source));
            result.nextStepFor[goal] = goal;
        }

        while (frontier.Count > 0)
        {
            (int x, int y) current = frontier.Dequeue();
            if (current.x == source.x && current.y == source.y)
            {
                result.success = true;
                result.steps = cost[current];
                result.nextStep = result.nextStepFor[source];
                // i checked cost.Keys.Count, seems good.
                return result;
            }

            if (cost[current] > maxLength)
            {
                Godot.GD.PrintErr($"Shortest path >{maxLength} length. Aborting search.");
                return result;
            }

            foreach ((int x, int y) neighbor in GridHelper.GetNeighbors(current))
            {
                // Filter neighbors. This search goes backwards, remember.
                if (!walkable((neighbor, current))) { continue; }

                int distance = GridHelper.Distance(current, neighbor);
                if (!cost.ContainsKey(neighbor) || cost[current] + distance < cost[neighbor])
                {
                    cost[neighbor] = cost[current] + distance;
                    frontier.EnqueueWithoutDuplicates(neighbor, cost[neighbor] + GridHelper.Distance(neighbor, source));

                    // To go to a goal, go from the neighbor to the current.
                    result.nextStepFor[neighbor] = current;
                }
            }
        }

        // Fail!        
        return result;
    }

    public static PathResult ShortestPath((int x, int y) source, (int x, int y) goal, Predicate<((int x, int y) from, (int x, int y) to)> walkable)
    {
        return ShortestPathToMany(source, new List<(int x, int y)>() { goal }, walkable);
    }

    public static PathResult ShortestPathToMany((int x, int y) source, IEnumerable<(int x, int y)> goals, Predicate<((int x, int y) from, (int x, int y) to)> walkable)
    {
        return new PathFinder() { source = source, goals = goals, walkable = walkable }.Run();
    }
}