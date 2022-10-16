using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class PlanarGenerator : LevelGenerator
{
    [Export] OpenSimplexNoise distortion;
    [Export] OpenSimplexNoise sizeVariance;
    PlanarGraph graph;
    List<Vector2> embedding = new List<Vector2>();

    public override Model Generate(Model model)
    {
        GenerateEmbedding();
        GenerateMap(model);
        AddSystems(model);
        GenerateEntities(model);
        return model;
    }

    public void GenerateEmbedding()
    {
        graph = graph ?? new PlanarGraph(20, 3, 7, false, 530);
        // embed graph in plane.
        embedding.Add(new Vector2(0, 0));

        List<int> layerIndex = new List<int> { 0 };
        for (int i = 1; i < graph.nodes; i++)
        {
            if (graph.subtreeDepth[i] == graph.subtreeDepth[i - 1])
            {
                layerIndex.Add(layerIndex[layerIndex.Count - 1] + 1);
            }
            else
            {
                layerIndex.Add(0);
            }
        }

        List<int> layerSize = new List<int> { 1 };
        int lastInWidestLayer = 0;
        for (int i = 1; i < graph.nodes; i++)
        {
            layerSize.Add(layerIndex[i] + 1);
            for (int j = 1; j <= layerIndex[i]; j++)
            {
                layerSize[i - j] += 1;
            }
            if (layerSize[i] >= layerSize[lastInWidestLayer])
            {
                lastInWidestLayer = i;
            }
        }

        GD.Print("aaaaaa", lastInWidestLayer);

        // abusing the fact that this will iterate in order of depth.
        for (int current = 1; current <= lastInWidestLayer; current++)
        {
            // Vector2 placement = new Vector2((float)(graph.subtreeDepth[current] * 100), layerIndex[current] * 100);
            Vector2 placement = new Vector2(graph.subtreeDepth[current] * 15, 0);
            placement = placement.Rotated(3.14f / 4 + 3.14f / 2 * (layerIndex[current] + 0.1f) / (layerSize[current] - 1 + 0.2f));
            embedding.Add(placement);
            if (!ValidPlacement(current, placement))
            {
                GD.Print("honk");
            }
        }

        // assume perfect nary.
        for (int current = lastInWidestLayer + 1; current < graph.nodes; current++)
        {
            int parent = graph.edges[current][0];
            float parentAngle = embedding[parent].Angle();

            Vector2 placement = new Vector2(graph.subtreeDepth[current] * 15, 0);
            placement = placement.Rotated(parentAngle + graph.subtreeChildren[parent].IndexOf(current) / 9f);
            embedding.Add(placement);
            if (!ValidPlacement(current, placement))
            {
                GD.Print("honk");
            }
        }

        for (int i = 0; i < 100; i++)
        {
            // GD.Print(i);
            AdjustEmbedding();
        }
    }

    // doesn't work lmao. gotta study how spring systems work.
    private void AdjustEmbedding()
    {
        List<Vector2> delta = new List<Vector2>();
        for (int node = 0; node < embedding.Count; node++)
        {
            delta.Add(Vector2.Zero);
            for (int other = 0; other < embedding.Count; other++)
            {
                if (other == node) { continue; }
                Vector2 vec = embedding[other] - embedding[node]; // vector towards the other.
                float len = Math.Max(Math.Abs(vec.x), Math.Abs(vec.y));
                if (graph.edges[node].Contains(other))
                {
                    float springDiff = len - 1;
                    delta[node] += vec.LimitLength(1) * springDiff * springDiff * Math.Sign(springDiff) * 0.0015f;
                }
                // else
                // {
                delta[node] -= vec.LimitLength(1) / len / len * 20;
                // delta[node] += vec * 0.05f;
                // }
            }
        }
        float scale = 1;
        foreach (Vector2 vec in delta)
        {
            if (3 / vec.Length() < scale)
            {
                GD.Print("wowow, vector of length", vec.Length());
                scale = 3 / vec.Length();
            }
        }
        for (int node = 0; node < embedding.Count; node++)
        {
            embedding[node] += delta[node] * scale;
        }
    }

    private bool ValidPlacement(int current, Vector2 placement)
    {
        int currentParent = graph.edges[current][0];
        for (int other = 0; other < current; other++)
        {
            if (other == currentParent) { continue; }
            foreach (int edge in graph.edges[other])
            {
                if (edge >= current) { continue; }
                if (edge == currentParent) { continue; }
                if (Geometry.SegmentIntersectsSegment2d(embedding[other], embedding[edge], placement, embedding[currentParent]) is object)
                {
                    GD.PrintS(embedding[other], embedding[edge], placement, embedding[currentParent]);
                    return false;
                }
            }
            // if (placement.DistanceTo(embedding[other]) < 10)
            // {
            //     return false;
            // }
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
            // SplatMap(model.Map, (int)embedding[node].x, (int)embedding[node].y, 2 + graph.subtreeDepth[node] % 2);
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    SplatMap(model.Map, (int)embedding[node].x + dx, (int)embedding[node].y + dy, 1);
                }
            }
        }
    }

    public (float, float) Warp(float x, float y)
    {
        if (distortion is null)
        {
            distortion = new OpenSimplexNoise();
            distortion.Period = 43.7f;
            distortion.Octaves = 2;
        }
        x += 12 * distortion.GetNoise2d(x, y);
        y += 12 * distortion.GetNoise2d(y, x);
        return (x, y);
    }

    public void SplatMap(SparseMatrix map, float x, float y, int tile)
    {
        if (sizeVariance is null)
        {
            sizeVariance = new OpenSimplexNoise();
            sizeVariance.Period = 7.3f;
            sizeVariance.Octaves = 8;
        }
        // float r = 0.5f;
        float r = 2 * (sizeVariance.GetNoise2d(x, y) + 1f) + 0.5f;
        for (float dx = -r; dx <= r; dx++)
        {
            for (float dy = -r; dy <= r; dy++)
            {
                (float aaax, float aaay) = Warp(x + dx, y + dy);
                map.SetCell((int)(aaax), (int)(aaay), tile);
            }
        }
    }

    public void GenerateEntities(Model model)
    {
        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
        Species partnerAxolotl = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;
        Species enemy = GD.Load<Resource>("res://Crawler/Model/Species/Enemy.tres") as Species;

        (float x, float y) = Warp(embedding[0].x, embedding[0].y);
        int spawnX = (int)x;
        int spawnY = (int)y;

        GD.PrintS(spawnX, spawnY);
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

    public override void AddSystems(Model model)
    {
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/FogOfWarSystem.cs").New() as Resource);
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/VisionSystem.cs").New() as Resource);
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/StateSystem.cs").New() as Resource);
    }
}
