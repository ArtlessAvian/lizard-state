using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Stores map information.
/// </summary>
// Parasitically based on TileMap.
// Would change, but its quite convenient.
public class CrawlerMap
{
    public TileMap map = new TileMap();

    ~CrawlerMap()
    {
        // TODO: This doesn't work the way I think it does.
        // GD.Print("DEEESSSSTRUUUCTTIOOOON");
        map.QueueFree();
    }

    public int GetCell(int x, int y)
    {
        return map.GetCell(x, y);
    }

    public bool TileIsWall((int x, int y) position)
    {
        return TileIsWall(map.GetCell(position.x, position.y));
    }

    public static bool TileIsWall(int id)
    {
        return id == -1 || id == 6;
    }
}
