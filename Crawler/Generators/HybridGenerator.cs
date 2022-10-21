using Godot;
using Godot.Collections;
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
    [Export] float hallwayCutoff = 0.02f;
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

    public override Model Generate(Model model)
    {
        AddSystems(model);
        GenerateMap(model);
        GenerateEntities(model);
        return model;
    }

    public void GenerateMap(Model model)
    {
        (int x, int y) start = (0, 0);
        for (int i = 0; i < 100; i++)
        {
            if (SampleNoise(start.x, i) < hallwayCutoff)
            {
                start.y = i;
                break;
            }
        }

        // Generate all the halls.
        {
            // This is just BFS, but if it fails to find anything, makes the hallways wider, until it can.
            var cost = new System.Collections.Generic.Dictionary<(int, int), int>();
            var pq = new SimplePriorityQueue<(int, int), (float, int)>();
            cost.Add(start, 0);
            pq.Enqueue(start, (0, 0));
            model.map.SetCell(start.x, start.y, 1);
            for (int i = 0; i < hallTiles; i++)
            {
                (int x, int y) current = pq.Dequeue();

                var neighbors = new (int, int)[] {
                    (current.x + 1, current.y),
                    (current.x - 1, current.y),
                    (current.x, current.y + 1),
                    (current.x, current.y - 1),
                    };
                foreach ((int x, int y) neighbor in neighbors)
                {
                    if (cost.ContainsKey(neighbor)) { continue; }
                    float sample = SampleNoise(neighbor.x, neighbor.y);

                    model.map.SetCell(neighbor.x, neighbor.y, 1);
                    pq.Enqueue(neighbor, (Mathf.Max(0, sample - hallwayCutoff), cost[current] + 1));
                    cost.Add(neighbor, cost[current] + 1);
                }
            }
        }

        // Generate entrance
        {
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
                    if (model.map.GetCell(neighbor.x, neighbor.y) != -1)
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

                GD.Print("Going sideways");
                // no tiles upwards. try left and right.
                var sideways = new (int, int)[] {
                    (current.x + 1, current.y),
                    (current.x - 1, current.y)
                };
                foreach ((int x, int y) neighbor in sideways)
                {
                    if (!tiles.Contains(neighbor) && model.map.GetCell(neighbor.x, neighbor.y) != -1)
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
                model.map.SetCell(x, y, 5);
            }
            if (tiles.Count == 1)
            {
                // bullshit it.
                foreach ((int x, int y) in tiles)
                {
                    if (SampleNoise(x - 1, y) < SampleNoise(x + 1, y))
                    {
                        model.map.SetCell(x - 1, y, 5);
                    }
                    else
                    {
                        model.map.SetCell(x + 1, y, 5);
                    }
                }
            }
        }

        // generate moss at (0, 0). I don't have a good idea where else to put it lol.
    }

    public void GenerateEntities(Model model)
    {
        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
        Species partnerAxolotl = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;
        Species enemy = GD.Load<Resource>("res://Crawler/Model/Species/Enemy.tres") as Species;

        int spawnX = 0;
        int spawnY = 0;

        foreach (Vector2 vec in model.map.GetUsedCellsById(5))
        {
            spawnX = (int)vec.x;
            spawnY = (int)vec.y + 1;
            break;
        }

        GD.PrintS(spawnX, spawnY);
        model.AddEntity(CreateEntity(playerTegu, (spawnX, spawnY), 0));
        model.GetEntity(0).isPlayer = true;
        model.AddEntity(CreateEntity(partnerAxolotl, (spawnX + 1, spawnY), 0));

        // model.AddEntity(new Entity(playerTegu, (spawnX, spawnY), 0));
        // model.AddEntity(new Entity(partnerAxolotl, (spawnX, spawnY+1), 0));

        // // model.AddEntity(new Entity(enemy, (0, 10), 1));
        // model.AddEntity(new Entity(enemy, (1, 20), 1));
        // model.AddEntity(new Entity(enemy, (2, 20), 1));

        var tiles = model.map.GetUsedCellsById(1);
        tiles.Shuffle();
        for (int i = 0; i < 10; i++)
        {
            Vector2 vec = (Vector2)tiles[i + 5];
            model.AddEntity(CreateEntity(enemy, ((int)vec.x, (int)vec.y), 1));
        }
    }

    public static Entity CreateEntity(Species species, (int x, int y) position, int team)
    {
        Entity entity = (Entity)GD.Load<CSharpScript>("res://Crawler/Model/Entity.cs").New();

        entity.SetSpecies(species);
        entity.position = position;
        entity.SetTeam(team);

        return entity;
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
