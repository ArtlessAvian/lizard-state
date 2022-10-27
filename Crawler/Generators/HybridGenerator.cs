using Godot;
using Priority_Queue;
using System;
using System.Collections.Generic;

// Hybrid of Planar and Noise.
// Probably will both be deleted by the time you read this.
// Haaaaa.
[Tool]
public class HybridGenerator : LevelGenerator
{
    [Export] private OpenSimplexNoise noise;
    [Export] float hallwayCutoff = 0.05f;
    [Export] float hallTiles = 1000;

    public HybridGenerator()
    {
        if (noise is null)
        {
            noise = new OpenSimplexNoise();
            noise.Octaves = 1;
            noise.Period = 10;
        }
    }

    public override Model Generate(Model model, Entity[] playerTeam)
    {
        AddSystems(model);
        GenerateMap(model.map);
        PlacePlayers(model, playerTeam);
        GenerateEntities(model);
        return model;
    }

    public override void GenerateMap(CrawlerMap map)
    {
        (int x, int y) start = SearchForHallway();

        HashSet<(int, int)> candidates = FindCandidateHallways(start);

        // locate "rooms" / points of interest.
        List<(int, int)> rooms = PlaceRooms(candidates, 15);
        GD.Print("Generated ", rooms.Count, " rooms.");

        // generate graph given rooms.
        (bool[,] edges, List<(int, int)> borders) = BuildGraph(candidates, rooms);

        // prune for subgraph with properties.

        // draw subgraph into model.map. (also add dead ends)
        // TODO: Temporary
        foreach ((int x, int y) in candidates)
        {
            map.SetCell(x, y, 1);
        }

        // draw a river

        GenerateEntrance(map, start);

        // generate moss at cave center. I don't have a good idea where else to put it lol.

        // debug drawing
        // foreach ((int x, int y) in borders)
        // {
        //     model.map.SetCell(x, y, 3);
        // }

        // for (int i = 0; i < rooms.Count; i++)
        // {
        //     for (int j = i + 1; j < rooms.Count; j++)
        //     {
        //         if (edges[i, j])
        //         {
        //             foreach ((int x, int y) tile in GridHelper.LineBetween(rooms[i], rooms[j]))
        //             {
        //                 model.map.SetCell(tile.x, tile.y, 2);
        //             }
        //         }
        //     }
        // }

        // foreach ((int x, int y) in rooms)
        // {
        //     model.map.SetCell(x, y, 4);
        // }
    }

    // returns /some/ random valid place for a hallway
    private (int x, int y) SearchForHallway()
    {
        // search downwards.
        for (int i = 0; i < 100; i++)
        {
            if (SampleNoise(0, i) < hallwayCutoff)
            {
                return (0, i);
            }
        }
        // ugh.
        GD.PrintErr("HybridGenerator.SearchForHallway: Failed to find hallway");
        return (0, 0);
    }

    private HashSet<(int, int)> FindCandidateHallways((int x, int y) start)
    {
        // This is just BFS, but if it fails to find anything, makes the hallways wider, until it can.
        var distance = new Dictionary<(int, int), int>();
        var pq = new SimplePriorityQueue<(int, int), (float, int)>();
        pq.Enqueue(start, (0, 0));
        for (int i = 0; i < hallTiles; i++)
        {
            (float _, int dist) = pq.GetPriority(pq.First);
            (int x, int y) current = pq.Dequeue();
            distance.Add(current, dist);

            // diagonals omitted, since they cause a visual discontinuity that i don't like
            // (discontinuity untested).
            // while players can move through walls diagonally (now), but noticing a thin diagonal passageway is awkward.
            var neighbors = new (int, int)[] {
                (current.x + 1, current.y),
                (current.x - 1, current.y),
                (current.x, current.y + 1),
                (current.x, current.y - 1),
                };
            foreach ((int x, int y) neighbor in neighbors)
            {
                if (distance.ContainsKey(neighbor)) { continue; }
                if (pq.Contains(neighbor)) { continue; }
                float sample = SampleNoise(neighbor.x, neighbor.y);

                pq.Enqueue(neighbor, (Mathf.Max(0, sample - hallwayCutoff), distance[current] + 1));
            }
        }

        return new HashSet<(int, int)>(distance.Keys);
    }

    // Generate at least maxRooms Rooms.
    private List<(int, int)> PlaceRooms(HashSet<(int, int)> candidates, int maxRooms)
    {
        List<(int, int)> rooms = new List<(int, int)>();
        var tileUsed = new HashSet<(int, int)>();
        int room = 0;
        foreach ((int x, int y) tile in candidates)
        {
            if (room >= maxRooms) { break; } // room++ at the end.
            if (tileUsed.Contains(tile)) { continue; }

            // room {room} centered on tile {tile}!
            rooms.Add(tile);
            tileUsed.Add(tile);

            // mark every room in n steps.
            HashSet<(int, int)> seen = new HashSet<(int, int)>() { tile };
            List<(int, int)> frontier = new List<(int, int)>() { tile };
            for (int step = 1; step < 10; step++)
            {
                List<(int, int)> nextFrontier = new List<(int, int)>();
                foreach ((int x, int y) current in frontier)
                {
                    var neighbors = new (int, int)[] {
                        (current.x - 1, current.y - 1),
                        (current.x - 1, current.y),
                        (current.x - 1, current.y + 1),
                        (current.x, current.y - 1),
                        (current.x, current.y + 1),
                        (current.x + 1, current.y - 1),
                        (current.x + 1, current.y),
                        (current.x + 1, current.y + 1),
                        };
                    foreach ((int x, int y) neighbor in neighbors)
                    {
                        if (seen.Contains(neighbor)) { continue; }
                        seen.Add(neighbor);
                        nextFrontier.Add(neighbor);
                        tileUsed.Add(neighbor);
                    }
                }
                frontier = nextFrontier;
            }

            room++;
        }

        return rooms;
    }

    private (bool[,] edges, List<(int, int)> borders) BuildGraph(HashSet<(int, int)> candidates, List<(int, int)> rooms)
    {
        // build voronoi, then get dual. 
        bool[,] edges = new bool[rooms.Count, rooms.Count];
        List<(int, int)> borders = new List<(int, int)>();

        // bfs from everywhere.
        List<(int, int)> frontier = new List<(int, int)>(rooms);
        Dictionary<(int, int), int> closestTo = new Dictionary<(int, int), int>();
        for (int i = 0; i < rooms.Count; i++)
        {
            closestTo.Add(rooms[i], i);
        }
        for (int i = 0; i < 40 && frontier.Count > 0; i++)
        {
            List<(int, int)> nextFrontier = new List<(int, int)>();
            foreach ((int x, int y) current in frontier)
            {
                int currentRoom = closestTo[current];

                var neighbors = new (int, int)[] {
                        (current.x - 1, current.y - 1),
                        (current.x - 1, current.y),
                        (current.x - 1, current.y + 1),
                        (current.x, current.y - 1),
                        (current.x, current.y + 1),
                        (current.x + 1, current.y - 1),
                        (current.x + 1, current.y),
                        (current.x + 1, current.y + 1),
                        };
                foreach ((int x, int y) neighbor in neighbors)
                {
                    if (!candidates.Contains(neighbor)) { continue; }
                    if (!closestTo.ContainsKey(neighbor))
                    {
                        // the usual BFS stuff.
                        closestTo.Add(neighbor, closestTo[current]);
                        nextFrontier.Add(neighbor);
                    }
                    else
                    {
                        int neighborRoom = closestTo[neighbor];
                        if (currentRoom != neighborRoom)
                        {
                            edges[currentRoom, neighborRoom] = true;
                            edges[neighborRoom, currentRoom] = true;
                            borders.Add(neighbor);
                        }
                    }
                }
            }
            frontier = nextFrontier;
        }

        return (edges, borders);
    }

    private void GenerateEntrance(CrawlerMap map, (int x, int y) start)
    {
        // TODO: use model.map.chunks to find uppermost tile?

        // From the start, march upwards until stuck.
        // replace tiles for cave entrance.
        HashSet<(int, int)> tiles = new HashSet<(int, int)>();
        List<(int, int)> frontier = new List<(int, int)>();
        frontier.Add(start);
        tiles.Add(start);
        for (int i = 0; i < 100 && frontier.Count > 0; i++)
        {
            (int x, int y) current = frontier[frontier.Count - 1];
            frontier.RemoveAt(frontier.Count - 1);

            // attempt to march upwards.
            var above = new (int, int)[] {
                (current.x, current.y - 1),
                (current.x + 1, current.y - 1),
                (current.x - 1, current.y - 1),
                };
            bool broke = false;
            foreach ((int x, int y) neighbor in above)
            {
                if (map.GetCell(neighbor.x, neighbor.y) != -1)
                {
                    frontier.Clear(); // dont care about stuff with greater y.
                    tiles.Clear();
                    frontier.Add(neighbor);
                    tiles.Add(neighbor);

                    broke = true;
                    break; // for reasons
                }
            }
            if (broke)
            {
                continue;
            }

            // no tiles upwards. try left and right.
            var sideways = new (int, int)[] {
                (current.x + 1, current.y),
                (current.x - 1, current.y)
            };
            foreach ((int x, int y) neighbor in sideways)
            {
                if (!tiles.Contains(neighbor) && map.GetCell(neighbor.x, neighbor.y) != -1)
                {
                    tiles.Add(neighbor);
                    frontier.Add(neighbor);
                }
            }
        }
        // exiting the for means that every tile in tiles has no above neighbor.
        // paint all of these as entrances.
        foreach ((int x, int y) in tiles)
        {
            map.SetCell(x, y, 5);
        }
        if (tiles.Count == 1)
        {
            // bullshit it.
            foreach ((int x, int y) in tiles)
            {
                if (SampleNoise(x - 1, y) < SampleNoise(x + 1, y))
                {
                    map.SetCell(x - 1, y, 5);
                }
                else
                {
                    map.SetCell(x + 1, y, 5);
                }
            }
        }
    }

    private void PlacePlayers(Model model, Entity[] playerTeam)
    {
        int spawnX = 0;
        int spawnY = 0;
        foreach (Vector2 vec in model.map.GetUsedCellsById(5))
        {
            spawnX = (int)vec.x;
            spawnY = (int)vec.y + 1;
            break;
        }

        GD.PrintS(spawnX, spawnY);
        playerTeam[0].position = (spawnX, spawnY);
        playerTeam[1].position = (spawnX + 1, spawnY);

        model.AddEntity(playerTeam[0]);
        model.AddEntity(playerTeam[1]);
    }

    public void GenerateEntities(Model model)
    {
        Species enemy = GD.Load<Resource>("res://Crawler/Model/Species/Enemy.tres") as Species;

        var tiles = model.map.GetUsedCellsById(1);
        tiles.Shuffle();
        for (int i = 0; i < 10; i++)
        {
            Vector2 vec = (Vector2)tiles[i + 5];
            model.AddEntity(enemy.BuildEntity(((int)vec.x, (int)vec.y), 1));
        }
    }

    public void AddSystems(Model model)
    {
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/FogOfWarSystem.cs").New() as Resource);
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/VisionSystem.cs").New() as Resource);
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/StateSystem.cs").New() as Resource);
    }


    // Internal Stuff

    // sample in units of tiles.
    public float SampleNoise(int x, int y)
    {
        float sample = noise.GetNoise2d(x, y);
        return sample * sample;
    }

    // public Vector2 FindNearbyHallway(int x, int y)
    // {
    //     (int x, int y) best = (x, y);
    //     float bestSample = SampleNoise(x, y);

    //     HashSet<(int, int)> visited = new HashSet<(int, int)>();
    //     SimplePriorityQueue<(int, int)> pq = new SimplePriorityQueue<(int, int)>();
    //     pq.Enqueue(best, bestSample);
    //     visited.Add(best);

    //     for (int i = 0; i < 5; i++)
    //     {
    //         (int x, int y) current = pq.Dequeue();

    //         var neighbors = new (int, int)[] {
    //             (current.x + 1, current.y),
    //             (current.x - 1, current.y),
    //             (current.x, current.y + 1),
    //             (current.x, current.y + 1),
    //             };
    //         foreach ((int x, int y) neighbor in neighbors)
    //         {
    //             if (visited.Contains(neighbor)) { continue; }
    //             float sample = SampleNoise(neighbor.x, neighbor.y);
    //             pq.Enqueue(neighbor, sample);
    //             visited.Add(neighbor);
    //             if (sample < bestSample)
    //             {
    //                 (best, bestSample) = (neighbor, sample);
    //             }
    //             GD.Print(neighbor.x, ",", neighbor.y);
    //         }
    //     }

    //     return new Vector2(best.x, best.y);
    // }
}
