// using System;
// using System.Collections.Generic;
// using Godot;
// using Priority_Queue;

// static class PathFinding
// {
//     /// <summary>
//     /// Finds the shortest path from one source to one goal.
//     /// <returns> steps, and the neighboring tile to move to from the source </returns>
//     /// </summary>
//     public static (int steps, (int x, int y) nextStep) ShortestPathTo((int x, int y) source, (int x, int y) goal, Predicate<((int x, int y) from, (int x, int y) to)> Walkable)
//     {
//         return ShortestPathToMany(
//             source,
//             new List<(int, int)>(){goal},
//             Walkable
//         );
//     }

//     /// <summary>
//     /// Finds the shortest path from one source to many goals.
//     /// <returns> steps, and the neighboring tile to move to from the source </returns>
//     /// </summary>
//     /// Actually, it searches from the many goals to the source ;)
//     /// This is a recurring theme.
//     /// Makes the heuristic easy.
//     public static (int steps, (int x, int y) nextStep) ShortestPathToMany((int x, int y) source, IEnumerable<(int x, int y)> goals, Predicate<((int x, int y) from, (int x, int y) to)> Walkable)
//     {
//         Dictionary<(int, int), int> cost = new Dictionary<(int, int), int>();
//         SimplePriorityQueue<(int, int)> frontier = new SimplePriorityQueue<(int, int)>();
//         // nextStep and not cameFrom because the search is backwards!
//         // Dictionary<(int, int), (int, int)> nextStep = new Dictionary<(int, int), (int, int)>();

//         // Imagine one source connected to all these sources with 0 distance.
//         // Therefore, this works.
//         foreach((int x, int y) goal in goals)
//         {
//             cost[goal] = 0;
//             frontier.Enqueue(goal, GridHelper.Distance(goal, source));
//         }

//         while (frontier.Count > 0)
//         {
//             (int x, int y) current = frontier.Dequeue();

//             foreach ((int x, int y) neighbor in GridHelper.GetNeighbors(current))
//             {
//                 // Filter neighbors. This search goes backwards, remember.
//                 if (!Walkable((neighbor, current))) { continue; }

//                 // HACK: Assumes the cost of all movements is 1
//                 // early exit (since the cost of all movements is 1)
//                 if (neighbor.x == source.x && neighbor.y == source.y)
//                 {
//                     return (cost[current] + 1, current);
//                 }

//                 // if (cost[current] > 10) { continue; }

//                 if (!cost.ContainsKey(neighbor) || cost[current] + 1 < cost[neighbor])
//                 {
//                     cost[neighbor] = cost[current] + 1;
//                     frontier.EnqueueWithoutDuplicates(neighbor, cost[neighbor] + GridHelper.Distance(neighbor, source));
//                 }
//             }
//         }

//         // Fail!        
//         return (Int32.MaxValue, source);
//     }
// }