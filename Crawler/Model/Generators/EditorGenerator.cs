using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class EditorGenerator : LevelGenerator
{
    PackedScene scene;

    public EditorGenerator(string scenePath)
    {
        scene = GD.Load<PackedScene>(scenePath);
    }

    public EditorGenerator(Dictionary dict) : this((string)dict["ScenePath"])
    {}

    public Model Generate(Model model)
    {
        model.generatorData = this.SaveToDict();
        GenerateMap(model);
        GenerateEntities(model);
        return model;
    }

    public void GenerateMap(Model model)
    {
        TileMap map = (TileMap)scene.Instance();

        model.map.map.Set("format", 1);
        model.map.map.Set(
            "tile_data",
            map.Get("tile_data")
        );

        map.QueueFree();
    }

    public void GenerateEntities(Model model)
    {
        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
        Species partnerAxolotl = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;
        Species enemy = GD.Load<Resource>("res://Crawler/Model/Species/Enemy.tres") as Species;

        model.AddEntity(new Entity(playerTegu, (0, 0), 0));
        model.AddEntity(new Entity(partnerAxolotl, (-2, -2), 0));

        // model.AddEntity(new Entity(enemy, (0, 10), 1));
        model.AddEntity(new Entity(enemy, (1, 20), 1));
        model.AddEntity(new Entity(enemy, (2, 20), 1));

        Array tiles = model.map.map.GetUsedCells();
        tiles.Shuffle();
        for (int i = 0; i < 10; i++)
        {
            Vector2 vec = (Vector2)tiles[i+5];
            model.AddEntity(new Entity(enemy, ((int)vec.x, (int)vec.y), 1));
        }
    }

    public Dictionary SaveToDict()
    {
        Dictionary dict = new Dictionary();
        dict["Type"] = "Editor";
        dict["ScenePath"] = scene.ResourcePath;
        return dict;
    }
}
