using Godot;
using Godot.Collections;
using System.Collections.Generic;


public class EditorGenerator : LevelGenerator
{
    [Export] PackedScene scene;

    public override Model Generate(Model model)
    {
        GenerateMap(model);
        AddSystems(model);
        GenerateEntities(model);

        FloorItem item = (FloorItem)GD.Load<CSharpScript>("res://Crawler/Model/FloorItem.cs").New();
        item.position = (4, 0);
        model.AddFloorItem(item);
        FloorItem item2 = (FloorItem)GD.Load<CSharpScript>("res://Crawler/Model/FloorItem.cs").New();
        item2.position = (-16, -7);
        model.AddFloorItem(item2);

        return model;
    }

    public override void GenerateMap(Model model)
    {
        TileMap map = scene.Instance<TileMap>();
        model.Map.ReadFromTilemap(map);
        map.QueueFree();
    }

    public void GenerateEntities(Model model)
    {
        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
        Species partnerAxolotl = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;
        Species partnerGator = GD.Load<Resource>("res://Crawler/Model/Species/PartnerGator.tres") as Species;

        Species enemy = GD.Load<Resource>("res://Crawler/Model/Species/Enemy.tres") as Species;
        Species enemy2 = GD.Load<Resource>("res://Crawler/Model/Species/Enemy2.tres") as Species;

        model.AddEntity(CreateEntity(playerTegu, (0, -1), 0));
        model.GetEntity(0).isPlayer = true;
        model.AddEntity(CreateEntity(partnerAxolotl, (-1, -1), 0));
        // model.AddEntity(CreateEntity(partnerGator, (0, 0), 0));

        // model.AddEntity(new Entity(enemy, (0, 10), 1));
        // model.AddEntity(new Entity(enemy, (1, 20), 1));
        // model.AddEntity(new Entity(enemy, (2, 20), 1));


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

        // Array tiles = model.Map.GetUsedCells();
        // tiles.Shuffle();
        // for (int i = 0; i < 10; i++)
        // {
        //     Vector2 vec = (Vector2)tiles[i+5];
        //     model.AddEntity(CreateEntity(enemy, ((int)vec.x, (int)vec.y), 1));
        //     GD.Print((int)vec.x, " ", (int)vec.y);
        // }
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

    public override void AddSystems(Model model)
    {
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/FogOfWarSystem.cs").New() as Resource);
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/VisionSystem.cs").New() as Resource);
        model.systems.Add(GD.Load<CSharpScript>("res://Crawler/Model/Systems/StateSystem.cs").New() as Resource);
    }
}
