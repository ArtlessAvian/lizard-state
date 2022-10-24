using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class NoiseGenerator : LevelGenerator
{
    int spawnX = 0;
    int spawnY = 0;

    public override Model Generate(Model model, Entity[] playerTeam)
    {
        GenerateMap(model.map);
        AddSystems(model);
        PlacePlayers(model, playerTeam);
        GenerateEntities(model);
        return model;
    }

    public override void GenerateMap(CrawlerMap map)
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
                        map.SetCell(x, y, (int)(3));
                        if (spawnX == 0 && spawnY == 0)
                        {
                            spawnX = x;
                            spawnY = y;
                        }
                    }
                    else
                    {
                        map.SetCell(x, y, (int)(sample * 1));
                    }
                }
            }
        }
    }

    public void PlacePlayers(Model model, Entity[] playerTeam)
    {
        spawnX = 0;
        spawnY = -2;
        playerTeam[0].position = (spawnX, spawnY);
        playerTeam[1].position = (spawnX, spawnY + 1);

        model.AddEntity(playerTeam[0]);
        model.AddEntity(playerTeam[1]);
    }

    public void GenerateEntities(Model model)
    {
        Species enemy = GD.Load<Resource>("res://Crawler/Model/Species/Enemy.tres") as Species;

        Array tiles = model.map.GetUsedCellsById(3);
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
}
