using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class PlanarGenerator : LevelGenerator
{
    [Export]
    PlanarGraph graph;

    List<Vector2> embedding = new List<Vector2>();

    public override Model Generate(Model model)
    {
        GenerateEmbedding();
        GenerateMap(model);
        GenerateEntities(model);
        return model;
    }

    public void GenerateEmbedding()
    {
        graph = graph ?? new PlanarGraph(15, 3, 5, false);
        // embed graph in plane.
        embedding.Add(new Vector2(0, 0));
        List<float> angle = new List<float>();

        angle.Add(3.14f / 2);

        // abusing the fact that this will iterate in order of depth.
        for (int current = 1; current < graph.nodes; current++)
        {
            int currentParent = graph.edges[current][0];
            for (int attempt = 1000; attempt > 0; attempt--)
            {
                Vector2 placement = new Vector2(15, 0);
                float my_angle = angle[currentParent] + (GD.Randf() - 0.5f) * 3.14f * 2;
                placement = placement.Rotated(my_angle);
                placement += embedding[currentParent];
                placement = placement.Snapped(Vector2.One);
                if (ValidPlacement(current, placement) || attempt == 1)
                {
                    if (attempt == 1) { GD.Print("gave up"); }
                    embedding.Add(placement);
                    angle.Add(my_angle);
                    break;
                }
            }
        }
        graph.DumpGraph();
        GD.Print(string.Join<Vector2>(" ", embedding));
    }

    private bool ValidPlacement(int current, Vector2 placement)
    {
        int currentParent = graph.edges[current][0];
        for (int other = 0; other < current; other++)
        {
            foreach (int edge in graph.edges[other])
            {
                if (edge >= current) { continue; }
                if (Geometry.SegmentIntersectsSegment2d(embedding[other], embedding[edge], placement, embedding[currentParent]) is object)
                {
                    return false;
                }
            }
            if (placement.DistanceTo(embedding[other]) < 8)
            {
                return false;
            }
        }
        return true;
    }

    public override void GenerateMap(Model model)
    {
        // draw all hallways
        for (int node = 0; node < graph.nodes; node++)
        {
            foreach (int neighbor in graph.edges[node])
            {
                if (neighbor > node) { continue; }
                foreach ((int x, int y) in GridHelper.LineBetween(((int)embedding[node].x, (int)embedding[node].y), ((int)embedding[neighbor].x, (int)embedding[neighbor].y)))
                {
                    SplatMap(model.Map, x, y, 1);
                }
            }
        }

        for (int node = 0; node < graph.nodes; node++)
        {
            SplatMap(model.Map, (int)embedding[node].x, (int)embedding[node].y, 2 + graph.subtreeDepth[node] % 2);
        }
    }

    static OpenSimplexNoise noise = new OpenSimplexNoise();
    public static void SplatMap(TileMap map, float x, float y, int tile)
    {
        x += 12 * noise.GetNoise2d(x, y);
        y += 12 * noise.GetNoise2d(y, x);
        float r = noise.GetNoise2d(x, y) + 1.5f;
        for (float dx = -r; dx <= r; dx++)
        {
            for (float dy = -r; dy <= r; dy++)
            {
                map.SetCell((int)(x + dx), (int)(y + dy), tile);
            }
        }
    }

    public void GenerateEntities(Model model)
    {
        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
        Species partnerAxolotl = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;
        Species enemy = GD.Load<Resource>("res://Crawler/Model/Species/Enemy.tres") as Species;

        int spawnX = 0;
        int spawnY = 0;
        model.AddEntity(CreateEntity(playerTegu, (spawnX, spawnY), 0));
        model.GetEntity(0).isPlayer = true;
        model.AddEntity(CreateEntity(partnerAxolotl, (spawnX, spawnY + 1), 0));

        // model.AddEntity(new Entity(playerTegu, (spawnX, spawnY), 0));
        // model.AddEntity(new Entity(partnerAxolotl, (spawnX, spawnY+1), 0));

        // // model.AddEntity(new Entity(enemy, (0, 10), 1));
        // model.AddEntity(new Entity(enemy, (1, 20), 1));
        // model.AddEntity(new Entity(enemy, (2, 20), 1));

        // Array tiles = model.Map.GetUsedCellsById(3);
        // tiles.Shuffle();
        // for (int i = 0; i < 10; i++)
        // {
        //     Vector2 vec = (Vector2)tiles[i + 5];
        //     model.AddEntity(CreateEntity(enemy, ((int)vec.x, (int)vec.y), 1));
        // }
    }

    public static Entity CreateEntity(Species species, (int x, int y) position, int team)
    {
        Entity entity = (Entity)GD.Load<CSharpScript>("res://Crawler/Model/Entity.cs").New();

        entity.SetSpecies(species);
        entity.position = position;
        entity.SetTeam(team);

        return entity;
    }
}
