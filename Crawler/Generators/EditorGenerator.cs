using Godot;
using Godot.Collections;
using System.Collections.Generic;


public class EditorGenerator : LevelGenerator
{
    [Export] PackedScene scene;

    public override Model Generate(Model model, Entity[] playerTeam)
    {
        GenerateMap(model.map);
        AddSystems(model);
        PlacePlayers(model, playerTeam);
        GenerateEntities(model);

        ItemData data = GD.Load<ItemData>("res://Crawler/Model/ItemData/Something.tres");
        FloorItem item = data.BuildInventoryItem().BuildFloorItem((4, 0));
        model.AddFloorItem(item);
        FloorItem item2 = data.BuildInventoryItem().BuildFloorItem((-16, -7));
        model.AddFloorItem(item2);

        return model;
    }

    public override void GenerateMap(CrawlerMap map)
    {
        TileMap data = scene.Instance<TileMap>();
        map.ReadFromTilemap(data);
        data.QueueFree();
    }

    public void PlacePlayers(Model model, Entity[] playerTeam)
    {
        playerTeam[0].position = (0, 1);
        playerTeam[1].position = (-1, -1);

        model.AddEntity(playerTeam[0]);
        model.AddEntity(playerTeam[1]);
    }

    public void GenerateEntities(Model model)
    {
        Species enemy = GD.Load<Resource>("res://Crawler/Model/Species/Enemy.tres") as Species;
        Species enemy2 = GD.Load<Resource>("res://Crawler/Model/Species/Enemy2.tres") as Species;

        model.AddEntity(enemy.BuildEntity((21, 10), 1));
        model.AddEntity(enemy.BuildEntity((8, 34), 1));
        model.AddEntity(enemy.BuildEntity((32, 47), 1));
        model.AddEntity(enemy.BuildEntity((-11, -5), 1));
        model.AddEntity(enemy.BuildEntity((35, 4), 1));
        model.AddEntity(enemy2.BuildEntity((17, -29), 1));
        model.AddEntity(enemy2.BuildEntity((-18, -12), 1));
        model.AddEntity(enemy2.BuildEntity((23, -17), 1));
        model.AddEntity(enemy2.BuildEntity((9, -25), 1));
        model.AddEntity(enemy2.BuildEntity((16, 16), 1));
    }

    public void AddSystems(Model model)
    {
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/FogOfWarSystem.cs").New() as Resource);
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/VisionSystem.cs").New() as Resource);
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/StateSystem.cs").New() as Resource);
    }
}
