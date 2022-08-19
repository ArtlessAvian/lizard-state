using Godot;
using Godot.Collections;
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
        graph = graph ?? new PlanarGraph(15, 4, 15);
        // embed graph in plane.
        embedding.Add(new Vector2(0, 0));

        // with brute force.
        int lastY = 0;
        int lastDepth = 0;
        // abusing the fact that this will iterate in order of depth.
        for (int current = 1; current < graph.nodes; current++)
        {
            int currentParent = graph.edges[current][0];
            for (int _ = 0; _ < 20; _++)
            {
                bool success = true;
                if (graph.subtreeDepth[current] != lastDepth) { lastY = 0; lastDepth = graph.subtreeDepth[current]; } else { lastY++; }
                Vector2 placement = new Vector2(graph.subtreeDepth[current] * 50, lastY * 50).Snapped(Vector2.One);
                // for (int other = 0; other < current; other++)
                // {
                //     foreach (int edge in graph.edges[other])
                //     {
                //         if (edge >= current) { continue; }
                //         if (Geometry.SegmentIntersectsSegment2d(embedding[other], embedding[edge], placement, embedding[currentParent]) is object)
                //         {
                //             success = false;
                //             break;
                //         }
                //     }
                //     if (!success) { break; }
                // }

                if (success || _ == 19)
                {
                    if (_ == 19) { GD.Print("gave up"); }
                    embedding.Add(placement);
                    break;
                }
            }
        }
        graph.DumpGraph();
        GD.Print(string.Join<Vector2>(" ", embedding));
    }

    public override void GenerateMap(Model model)
    {
        // draw all hallways
        for (int node = 0; node < graph.nodes; node++)
        {
            foreach (int neighbor in graph.edges[node])
            {
                if (embedding[node].x > embedding[neighbor].x) ;
                for (int x = (int)embedding[node].x; x < embedding[neighbor].x; x++)
                {
                    int y = (int)(embedding[node].y + (embedding[neighbor].y - embedding[node].y) / (embedding[neighbor].x - embedding[node].x) * (x - embedding[node].x));
                    model.Map.SetCell(x, y, graph.subtreeDepth[node]);
                }
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
