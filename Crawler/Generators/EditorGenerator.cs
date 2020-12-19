using Godot;
using System.Collections.Generic;

public class EditorGenerator
{
    public static void GenerateMap(Model model, List<ModelEvent> eventQueue, string argument)
    {
        PackedScene scene = GD.Load<PackedScene>(argument);
        TileMap map = (TileMap)scene.Instance();

        model.map.Set("format", 1);
        model.map.Set(
            "tile_data",
            map.Get("tile_data")
        );
    }

    public static void GenerateEntities(Model model, List<ModelEvent> eventQueue, string argument)
    {
        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
        Species partnerAxolotl = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;

        model.AddEntity(eventQueue, new Entity(playerTegu, (0, 0)));
        for (int i = 0; i < 5; i++)
        {
            model.AddEntity(eventQueue, new Entity(partnerAxolotl, (i, i-4)));
        }
    }
}
