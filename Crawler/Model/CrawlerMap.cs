using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Stores map information.
/// </summary>
public class CrawlerMap : TileMap
{
    public CrawlerMap()
    {
    }

    // Assume this is always true.
    // Hopefully ez fix if its an issue.
    public static bool TileIsWall(int id)
    {
        return id == -1 || id == 6;
    }
}
