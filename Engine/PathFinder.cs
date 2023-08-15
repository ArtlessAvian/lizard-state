using System;
using System.Collections.Generic;
using Priority_Queue;

namespace LizardState.Engine
{
    // Use an object initializer, then call run().
    // There's also convenience functions.
    public class PathFinder
    {
        // Parameters before a run.
        public AbsolutePosition source;
        public IEnumerable<AbsolutePosition> goals;
        public Predicate<(AbsolutePosition from, AbsolutePosition to)> walkable;

        // Optional.
        public int maxLength = 22;
        // Not a lot, but also, kind of a lot. (vision radius is ~8)
        // In practice, searches ~100 tiles when triggered. Verify with cost.Count.

        // Factoring heuristic early exit, in the worst case (empty field) searches ~500 tiles?
        // Assume source and destination are the same.
        // 11 steps away from the source, estimated path cost is 22. One more step and it terminates.
        // The entire square searched should be 23 tiles across.

        // Results, ofc.
        public class PathResult
        {
            public bool success = false;
            public float steps = -1;
            public AbsolutePosition nextStep; // Convenience. nextStepFor[source]
            public Dictionary<AbsolutePosition, AbsolutePosition> nextStepFor;
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
            Dictionary<AbsolutePosition, int> cost = new Dictionary<AbsolutePosition, int>();
            SimplePriorityQueue<AbsolutePosition> frontier = new SimplePriorityQueue<AbsolutePosition>();

            result.nextStepFor = new Dictionary<AbsolutePosition, AbsolutePosition>();

            // Imagine one source connected to all these sources with 0 distance.
            // Therefore, this works.
            foreach (AbsolutePosition goal in goals)
            {
                cost[goal] = 0;
                frontier.Enqueue(goal, GridHelper.Distance(goal, source));
                result.nextStepFor[goal] = goal;
            }

            while (frontier.Count > 0)
            {
                AbsolutePosition current = frontier.Dequeue();
                if (current.x == source.x && current.y == source.y)
                {
                    result.success = true;
                    result.steps = cost[current];
                    result.nextStep = result.nextStepFor[source];
                    return result;
                }

                if (cost[current] + GridHelper.Distance(current, source) > maxLength)
                {
                    // Assume the heuristic (raw distance) is admissible (always underestimates).
                    // Everything in the frontier will not lead to a path, so we can stop.
                    // Godot.GD.PrintErr("Search length hit! ", cost.Count);
                    return result;
                }

                foreach (AbsolutePosition neighbor in GridHelper.GetNeighbors(current))
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

        public static PathResult ShortestPath(AbsolutePosition source, AbsolutePosition goal, Predicate<(AbsolutePosition from, AbsolutePosition to)> walkable)
        {
            return ShortestPathToMany(source, new List<AbsolutePosition>() { goal }, walkable);
        }

        public static PathResult ShortestPathToMany(AbsolutePosition source, IEnumerable<AbsolutePosition> goals, Predicate<(AbsolutePosition from, AbsolutePosition to)> walkable)
        {
            return new PathFinder() { source = source, goals = goals, walkable = walkable }.Run();
        }
    }
}