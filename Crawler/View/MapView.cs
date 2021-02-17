using Godot;
using System;
using System.Collections.Generic;

public class MapView : TileMap
{
    Dictionary<int, ((int, int), int[,])> entityVisions = new Dictionary<int, ((int, int), int[,])>();

    public override void _Ready()
    {
        GetNode<TileMap>("Visibility").Visible = true;
    }

    public void AddVision(int seeer, (int x, int y) center, int[,] tiles)
    {
        int r = tiles.GetLength(0) / 2; // floored

        for (int dy = -r; dy <= r; dy++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int tile = tiles[dx + r, dy + r];
                if (tile != -2) {
                    this.SetCell(center.x + dx, center.y + dy, tile);
                }
            }
        }

        // store the new vision into a dictionary.
        entityVisions[seeer] = (center, tiles);
        RefreshVision();
    }

    public void RefreshVision()
    {
        TileMap visibility = GetNode<TileMap>("Visibility");
        
        // Clear,
        foreach (Vector2 cell in visibility.GetUsedCellsById(2))
        {
            visibility.SetCellv(cell, 1);
        }

        // and refresh from dictionary.
        foreach (((int x, int y) center, int[,] tiles) in entityVisions.Values)
        {
            int r = tiles.GetLength(0) / 2; // floored
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    int tile = tiles[dx + r, dy + r];
                    if (tile != -2) {
                        visibility.SetCell(center.x + dx, center.y + dy, 2);
                    }
                }
            }
        }
    }
}
