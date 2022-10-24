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

        FloorItem item = (FloorItem)GD.Load<CSharpScript>("res://Crawler/Model/FloorItem.cs").New();
        item.position = (4, 0);
        model.AddFloorItem(item);
        FloorItem item2 = (FloorItem)GD.Load<CSharpScript>("res://Crawler/Model/FloorItem.cs").New();
        item2.position = (-16, -7);
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

        model.AddEntity(CreateEntity(enemy, (21, 10), 1));
        model.AddEntity(CreateEntity(enemy, (8, 34), 1));
        model.AddEntity(CreateEntity(enemy, (32, 47), 1));
        model.AddEntity(CreateEntity(enemy, (-11, -5), 1));
        model.AddEntity(CreateEntity(enemy, (35, 4), 1));
        model.AddEntity(CreateEntity(enemy2, (17, -29), 1));
        model.AddEntity(CreateEntity(enemy2, (-18, -12), 1));
        model.AddEntity(CreateEntity(enemy2, (23, -17), 1));
        model.AddEntity(CreateEntity(enemy2, (9, -25), 1));
        model.AddEntity(CreateEntity(enemy2, (16, 16), 1));
    }

    // TODO: duplicated code! from NoiseGenerator.
    public Entity CreateEntity(Species species, (int x, int y) position, int team)
    {
        // I forgot why I'm doing this.
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
