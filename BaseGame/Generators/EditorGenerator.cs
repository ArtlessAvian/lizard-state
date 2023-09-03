using Godot;
using Godot.Collections;
using System.Collections.Generic;
using LizardState.Engine;


public class EditorGenerator : LevelGenerator
{
    [Export] PackedScene scene;

    public override Model Generate(Model model, Entity[] playerTeam)
    {
        GenerateMap(model.map.tiles);
        AddSystems(model);
        PlacePlayers(model, playerTeam);
        GenerateEntities(model);

        ItemData data = GD.Load<ItemData>("res://BaseGame/ItemData/Something.tres");
        FloorItem item = data.BuildInventoryItem().BuildFloorItem(new AbsolutePosition(4, 0));
        model.AddFloorItem(item);
        FloorItem item2 = data.BuildInventoryItem().BuildFloorItem(new AbsolutePosition(-16, -7));
        model.AddFloorItem(item2);

        return model;
    }

    public override void GenerateMap(SparseMatrix tiles)
    {
        TileMap data = scene.Instance<TileMap>();
        tiles.ReadFromTilemap(data);
        data.QueueFree();
    }

    public void PlacePlayers(Model model, Entity[] playerTeam)
    {
        playerTeam[0].position = new AbsolutePosition(0, 1);
        playerTeam[1].position = new AbsolutePosition(-1, -1);

        model.AddEntity(playerTeam[0]);
        model.AddEntity(playerTeam[1]);
    }

    public void GenerateEntities(Model model)
    {
        Species enemy = GD.Load<Resource>("res://BaseGame/Species/Enemy.tres") as Species;
        Species enemy2 = GD.Load<Resource>("res://BaseGame/Species/Enemy2.tres") as Species;

        model.AddEntity(enemy.BuildEntity(new AbsolutePosition(21, 10), 1));
        model.AddEntity(enemy.BuildEntity(new AbsolutePosition(8, 34), 1));
        model.AddEntity(enemy.BuildEntity(new AbsolutePosition(32, 47), 1));
        model.AddEntity(enemy.BuildEntity(new AbsolutePosition(-11, -5), 1));
        model.AddEntity(enemy.BuildEntity(new AbsolutePosition(35, 4), 1));
        model.AddEntity(enemy2.BuildEntity(new AbsolutePosition(17, -29), 1));
        model.AddEntity(enemy2.BuildEntity(new AbsolutePosition(-18, -12), 1));
        model.AddEntity(enemy2.BuildEntity(new AbsolutePosition(23, -17), 1));
        model.AddEntity(enemy2.BuildEntity(new AbsolutePosition(9, -25), 1));
        model.AddEntity(enemy2.BuildEntity(new AbsolutePosition(16, 16), 1));
    }

    public void AddSystems(Model model)
    {
        model.systems.Add(GD.Load<CSharpScript>("res://BaseGame/Systems/FogOfWarSystem.cs").New() as CrawlerSystem);
        model.systems.Add(GD.Load<CSharpScript>("res://BaseGame/Systems/VisionSystem.cs").New() as CrawlerSystem);
        model.systems.Add(GD.Load<CSharpScript>("res://BaseGame/Systems/StateSystem.cs").New() as CrawlerSystem);
    }
}
