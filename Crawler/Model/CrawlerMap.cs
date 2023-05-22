using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Stores map information.
/// </summary>
// Parasitically based on TileMap.
// Would change, but its quite convenient.
[Tool]
public class CrawlerMap : SparseMatrix
{
    public CrawlerMap()
    {
    }

    // public new int GetCell(int x, int y)
    // {
    //     return base.GetCell(x/2, y/2);
    // }

    public bool TileIsWall(AbsolutePosition position)
    {
        return TileIsWall(this.GetCell(position.x, position.y));
    }

    public static bool TileIsWall(int id)
    {
        return id == -1 || id == 6;
    }
}
