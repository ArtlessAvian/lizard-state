using Godot;
using System;
using System.Collections.Generic;

public class MapView : Node2D
{
    Dictionary<int, Vector3[]> entityVisions = new Dictionary<int, Vector3[]>();

    bool dirty = true;
    public override void _Process(float delta)
    {
        if (dirty)
        {
            dirty = false;
            RefreshVision();
        }
    }

    public void AddVision(Godot.Collections.Dictionary ev)
    {
        AddVision((int)ev["subject"], (Vector3[])ev["tiles"]);
    }

    public void AddVision(int seeer, Vector3[] tiles)
    {
        TileMap floors = GetNode<TileMap>("Floors");
        TileMap walls = GetNode<TileMap>("Walls");

        WriteVision(tiles, floors, walls);

        // For updating current.
        entityVisions[seeer] = tiles;
        dirty = true;
    }

    public void RefreshVision()
    {
        TileMap floorsVisible = GetNode<TileMap>("Floors/Visible");
        TileMap wallsVisible = GetNode<TileMap>("Walls/Visible");

        floorsVisible.Clear();
        wallsVisible.Clear();
        // Refresh from dictionary.
        foreach (int seeer in entityVisions.Keys)
        {
            Vector3[] tiles = entityVisions[seeer];
            WriteVision(tiles, floorsVisible, wallsVisible);
        }
    }

    public void WriteVision(Vector3[] tiles, TileMap floors, TileMap walls)
    {
        foreach (Vector3 tile in tiles)
        {
            WriteFloorOrWall((int)(tile.x), (int)(tile.y), (int)tile.z, floors, walls);
        }
    }

    public void WriteFloorOrWall(int x, int y, int tile, TileMap floors, TileMap walls)
    {
        if (tile != -2)
        {
            if (CrawlerMap.TileIsWall(tile))
            {
                walls.SetCell(x, y, tile != -1 ? tile : 8);
            }
            else
            {
                floors.SetCell(x, y, tile);
            }
        }
        if (tile != -2)
        {
            GetNode<TileMap>("Minimap/Minimap").SetCell(x, y, tile != -1 ? tile : 8);
        }
    }

    public void ModelSync(CrawlerMap map, FogOfWarSystem fog)
    {
        TileMap floors = GetNode<TileMap>("Floors");
        TileMap walls = GetNode<TileMap>("Walls");

        foreach (Vector2 vec in fog.GetUsedCells())
        {
            WriteFloorOrWall((int)vec.x, (int)vec.y, map.GetCell((int)vec.x, (int)vec.y), floors, walls);
        }

        foreach (int seer in fog.lastVision.Keys)
        {
            AddVision(seer, fog.lastVision[seer]);
        }
    }
}
