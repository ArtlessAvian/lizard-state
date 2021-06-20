using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class NoiseGenerator : LevelGenerator
{
    int spawnX = 0;
    int spawnY = 0;

    public NoiseGenerator()
    {
    }

    public Model Generate(Model model)
    {
        model.generatorData = this.SaveToDict();
        GenerateMap(model);
        GenerateEntities(model);
        return model;
    }

    public void GenerateMap(Model model)
    {
        OpenSimplexNoise noise = new OpenSimplexNoise();
        noise.Period = 5;
        noise.Octaves = 1;

        for (int x = -50; x < 50; x++)
        {
            for (int y = -50; y < 50; y++)
            {
                float sample = noise.GetNoise2d(x, y);
                if (sample > 0)
                {
                    if (sample > 0.5)
                    {
                        model.Map.SetCell(x, y, (int)(3));
                        if (spawnX == 0 && spawnY == 0)
                        {
                            spawnX = x;
                            spawnY = y;
                        }
                    }
                    else
                    {
                        model.Map.SetCell(x, y, (int)(sample * 1));
                    }
                }
            }
        }
    }

    public void GenerateEntities(Model model)
    {
        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
        Species partnerAxolotl = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;
        Species enemy = GD.Load<Resource>("res://Crawler/Model/Species/Enemy.tres") as Species;

        model.AddEntity(playerTegu.CreateEntity((spawnX, spawnY), 0));
        model.AddEntity(partnerAxolotl.CreateEntity((spawnX, spawnY+1), 0));
        
        // model.AddEntity(new Entity(playerTegu, (spawnX, spawnY), 0));
        // model.AddEntity(new Entity(partnerAxolotl, (spawnX, spawnY+1), 0));

        // // model.AddEntity(new Entity(enemy, (0, 10), 1));
        // model.AddEntity(new Entity(enemy, (1, 20), 1));
        // model.AddEntity(new Entity(enemy, (2, 20), 1));

        Array tiles = model.Map.GetUsedCellsById(3);
        tiles.Shuffle();
        for (int i = 0; i < 10; i++)
        {
            Vector2 vec = (Vector2)tiles[i+5];
            model.AddEntity(enemy.CreateEntity(((int)vec.x, (int)vec.y), 1));
        }
    }

    public Dictionary SaveToDict()
    {
        Dictionary dict = new Dictionary();
        dict["Type"] = "Noise";
        return dict;
    }
}
