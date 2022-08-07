using Godot;
using System;
using System.Collections.Generic;

public class MapView : Node2D
{
    Dictionary<int, Vector2> entityVisions = new Dictionary<int, Vector2>();
    Dictionary<int, int[]> entityVisions2 = new Dictionary<int, int[]>();

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
        AddVision((int)ev["subject"], (Vector2)ev["center"], (int[])ev["tiles"]);
    }

    public void AddVision(int seeer, Vector2 center, int[] tiles1d)
    {
        TileMap floors = GetNode<TileMap>("Floors");
        TileMap walls = GetNode<TileMap>("Walls");

        WriteVision(center, tiles1d, floors, walls);

        // For updating current.
        entityVisions[seeer] = center;
        entityVisions2[seeer] = tiles1d;
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
            Vector2 center = entityVisions[seeer];
            int[] tiles1d = entityVisions2[seeer];

            WriteVision(center, tiles1d, floorsVisible, wallsVisible);
        }
    }

    public void WriteVision(Vector2 center, int[] tiles1d, TileMap floors, TileMap walls)
    {
        int sidelen = (int)Math.Sqrt(tiles1d.Length);
        int r = sidelen / 2; // floored

        for (int dx = -r; dx <= r; dx++)
        {
            for (int dy = -r; dy <= r; dy++)
            {
                int tile = tiles1d[(dx + r) * sidelen + dy + r];
                WriteFloorOrWall((int)(center.x + dx), (int)(center.y + dy), tile, floors, walls);
            }
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
    }

    public void SyncRevealed(CrawlerMap map, FogOfWarSystem fog)
    {
        TileMap floors = GetNode<TileMap>("Floors");
        TileMap walls = GetNode<TileMap>("Walls");

        foreach (Vector2 vec in fog.GetUsedCells())
        {
            WriteFloorOrWall((int)vec.x, (int)vec.y, map.GetCell((int)vec.x, (int)vec.y), floors, walls);
        }
    }
}
