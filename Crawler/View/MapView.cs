using Godot;
using System;
using System.Collections.Generic;

public class MapView : TileMap
{
    Dictionary<int, ((int, int), int[,])> entityVisions = new Dictionary<int, ((int, int), int[,])>();

    public void AddVision(int seeer, (int x, int y) center, int[,] tiles)
    {
        AddHistory(center, tiles);
        // store the new vision into a dictionary.
        entityVisions[seeer] = (center, tiles);
        RefreshVision();
    }

    public void AddHistory((int x, int y) center, int[,] tiles)
    {
        int r = tiles.GetLength(0) / 2; // floored

        for (int dy = -r; dy <= r; dy++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int tile = tiles[dx + r, dy + r];
                if (tile != -2) {
                    this.SetCell(center.x + dx, center.y + dy, tile >= 0 ? tile : 6);
                }
            }
        }
    }

    public void RefreshVision()
    {
        TileMap visible = GetNode<TileMap>("Visible");
        TileMap walls = GetNode<TileMap>("VisibleWalls");
        
        visible.Clear();
        walls.Clear();
        // Refresh from dictionary.
        foreach (((int x, int y) center, int[,] tiles) in entityVisions.Values)
        {
            int r = tiles.GetLength(0) / 2; // floored
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    int tile = tiles[dx + r, dy + r];
                    if (tile != -2) {
                        // if (Map.TileIsWall(tile))
                        // {
                            // walls.SetCell(center.x + dx, center.y + dy, tile);
                        // }
                        // else
                        // {
                        visible.SetCell(center.x + dx, center.y + dy, tile >= 0 ? tile : 6);
                        // }
                    }
                }
            }
        }
    }
}
