using Godot;
using System;
using System.Collections.Generic;

public class MapView : Node2D
{
    Dictionary<int, ((int, int), int[,])> entityVisions = new Dictionary<int, ((int, int), int[,])>();

    public void AddVision(int seeer, (int x, int y) center, int[,] tiles)
    {
        AddHistory(center, tiles);
        // store the new vision, and then refresh from all visions.
        // TODO: replace this by only clearing the old vision, then refreshing any overlap with other visions.
        entityVisions[seeer] = (center, tiles);
        RefreshVision();
    }

    public void AddHistory((int x, int y) center, int[,] tiles)
    {
        TileMap floors = GetNode<TileMap>("Floors");
        TileMap walls = GetNode<TileMap>("Walls");

        int r = tiles.GetLength(0) / 2; // floored

        for (int dy = -r; dy <= r; dy++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int tile = tiles[dx + r, dy + r];
                if (tile != -2)
                {
                    if (CrawlerMap.TileIsWall(tile))
                    {
                        walls.SetCell(center.x + dx, center.y + dy, tile != -1 ? tile : 6);
                    }
                    else
                    {
                        floors.SetCell(center.x + dx, center.y + dy, tile);
                    }
                }
            }
        }
    }

    public void RefreshVision()
    {
        TileMap floorsVisible = GetNode<TileMap>("Floors/Visible");
        TileMap wallsVisible = GetNode<TileMap>("Walls/Visible");
        
        floorsVisible.Clear();
        wallsVisible.Clear();
        // Refresh from dictionary.
        foreach (((int x, int y) center, int[,] tiles) in entityVisions.Values)
        {
            int r = tiles.GetLength(0) / 2; // floored
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    int tile = tiles[dx + r, dy + r];
                    if (tile != -2)
                    {
                        if (CrawlerMap.TileIsWall(tile))
                        {
                            wallsVisible.SetCell(center.x + dx, center.y + dy, tile != -1 ? tile : 6);
                        }
                        else
                        {
                            floorsVisible.SetCell(center.x + dx, center.y + dy, tile);
                        }
                    }
                }
            }
        }
    }
}
