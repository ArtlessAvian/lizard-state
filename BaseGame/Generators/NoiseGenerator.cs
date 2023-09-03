using Godot;
using Godot.Collections;
using System.Collections.Generic;
using LizardState.Engine;

public class NoiseGenerator : LevelGenerator
{
    int spawnX = 0;
    int spawnY = 0;

    public override Model Generate(Model model, Entity[] playerTeam)
    {
        GenerateMap(model.map.tiles);
        AddSystems(model);
        PlacePlayers(model, playerTeam);
        GenerateEntities(model);
        return model;
    }

    public override void GenerateMap(SparseMatrix tiles)
    {
        OpenSimplexNoise noise = new OpenSimplexNoise();
        noise.Period = 6;
        noise.Octaves = 1;

        noise.Seed = 3;

        for (int x = -50; x < 50; x++)
        {
            for (int y = -50; y < 50; y++)
            {
                float sample = noise.GetNoise2d(x, y);
                if (sample > 0)
                {
                    if (sample > 0.5)
                    {
                        tiles.SetCell(x, y, (int)(3));
                        if (spawnX == 0 && spawnY == 0)
                        {
                            spawnX = x;
                            spawnY = y;
                        }
                    }
                    else
                    {
                        tiles.SetCell(x, y, (int)(sample * 1));
                    }
                }
            }
        }
    }

    public void PlacePlayers(Model model, Entity[] playerTeam)
    {
        spawnX = 0;
        spawnY = -2;
        playerTeam[0].position = new AbsolutePosition(spawnX, spawnY);
        playerTeam[1].position = new AbsolutePosition(spawnX, spawnY + 1);

        model.AddEntity(playerTeam[0]);
        model.AddEntity(playerTeam[1]);
    }

    public void GenerateEntities(Model model)
    {
        Species enemy = GD.Load<Resource>("res://BaseGame/Species/Enemy.tres") as Species;

        List<AbsolutePosition> tiles = new List<AbsolutePosition>(model.map.tiles.GetUsedCellsByIdIterator(3));
        // tiles.Shuffle();
        for (int i = 0; i < 10; i++)
        {
            AbsolutePosition vec = tiles[(int)(GD.Randi() % tiles.Count)];
            model.AddEntity(enemy.BuildEntity(vec, 1));
        }
    }

    public void AddSystems(Model model)
    {
        model.systems.Add(GD.Load<CSharpScript>("res://BaseGame/Systems/FogOfWarSystem.cs").New() as CrawlerSystem);
        model.systems.Add(GD.Load<CSharpScript>("res://BaseGame/Systems/VisionSystem.cs").New() as CrawlerSystem);
        model.systems.Add(GD.Load<CSharpScript>("res://BaseGame/Systems/StateSystem.cs").New() as CrawlerSystem);
    }
}
