using Godot;
using System;
using System.Collections.Generic;

public class MapView : Node2D
{
    Dictionary<int, Vector2> entityVisions = new Dictionary<int, Vector2>();
    Dictionary<int, int[]> entityVisions2 = new Dictionary<int, int[]>();

    public void AddVision(Godot.Collections.Dictionary ev)
    {
        // GD.Print(ev);
        int seeer = (int)ev["subject"];
        Vector2 center = (Vector2)ev["center"];
        int[] tiles1d = (int[])ev["tiles"];

        AddVision(seeer, center, tiles1d);
    }

    public void AddVision(int seeer, Vector2 center, int[] tiles1d)
    {
        AddHistory(center, tiles1d);
        // store the new vision, and then refresh from all visions.
        // TODO: replace this by only clearing the old vision, then refreshing any overlap with other visions.
        entityVisions[seeer] = center;
        entityVisions2[seeer] = tiles1d;
        RefreshVision();
    }

    public void AddHistory(Vector2 center, int[] tiles1d)
    {
        TileMap floors = GetNode<TileMap>("Floors");
        TileMap walls = GetNode<TileMap>("Walls");

        int[,] tiles = Squareify1d(tiles1d);
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
                        walls.SetCell((int)(center.x + dx), (int)(center.y + dy), tile != -1 ? tile : 6);
                    }
                    else
                    {
                        floors.SetCell((int)(center.x + dx), (int)(center.y + dy), tile);
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
        foreach (int seeer in entityVisions.Keys)
        {
            Vector2 center = entityVisions[seeer];
            int[,] tiles = Squareify1d(entityVisions2[seeer]);

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
                            wallsVisible.SetCell((int)(center.x + dx), (int)(center.y + dy), tile != -1 ? tile : 6);
                        }
                        else
                        {
                            floorsVisible.SetCell((int)(center.x + dx), (int)(center.y + dy), tile);
                        }
                    }
                }
            }
        }
    }

    public int[,] Squareify1d(int[] tiles1d)
    {
        int sidelen = (int)Math.Sqrt(tiles1d.Length);
        int[,] tiles = new int[sidelen,sidelen];
        for (int i = 0; i < tiles1d.Length; i++)
        {
            tiles[i / sidelen, i % sidelen] = tiles1d[i];
        }
        return tiles;
    }
}
