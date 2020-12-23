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

    public Model Generate(List<ModelEvent> eventQueue)
    {
        Model model = new Model(eventQueue, this.SaveToDict());
        GenerateMap(model, eventQueue);
        GenerateEntities(model, eventQueue);
        return model;
    }

    public void GenerateMap(Model model, List<ModelEvent> eventQueue)
    {
        TileMap map = (TileMap)scene.Instance();

        model.map.Set("format", 1);
        model.map.Set(
            "tile_data",
            map.Get("tile_data")
        );
    }

    public void GenerateEntities(Model model, List<ModelEvent> eventQueue)
    {
        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
        Species partnerAxolotl = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;

        model.AddEntity(eventQueue, new Entity(playerTegu, (0, 0), 0));
        model.AddEntity(eventQueue, new Entity(partnerAxolotl, (2, 2), 0));
    }

    public Dictionary SaveToDict()
    {
        Dictionary dict = new Dictionary();
        dict["Type"] = "Editor";
        dict["ScenePath"] = scene.ResourcePath;
        return dict;
    }
}
