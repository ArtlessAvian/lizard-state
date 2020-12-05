using Godot;
using System;
using System.Collections.Generic;

// Super privlidged node, sets the model up from editor info.
public class EditorGenerator : Node
{
    public override void _Ready()
    {
        GenerateMap();
        GenerateEntities();
        QueueFree();
    }

    void GenerateMap()
    {
        Crawler crawler = GetParent<Crawler>();

        crawler.model.map.Set("format", 1);
        crawler.model.map.Set(
            "tile_data",
            crawler.GetNode<TileMap>("Map").Get("tile_data")
        );
    }

    void GenerateEntities()
    {
        Crawler crawler = GetParent<Crawler>();
        List<ModelEvent> eventQueue = crawler.eventQueue;

        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
        Species partnerAxolotl = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;

        crawler.model.AddEntity(eventQueue, new Entity(playerTegu, (0, 0)));
        for (int i = 0; i < 5; i++)
        {
            crawler.model.AddEntity(eventQueue, new Entity(partnerAxolotl, (i, i-4)));
        }
    }
}
