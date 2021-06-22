using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Stores map and vision information.
/// Maybe ask the 
/// </summary>
public class CrawlerMap : TileMap
{
    // hehe parasitic inheritance.
    // creates orphan nodes >:/
    public TileMap fog;

    private const int REVEALED = 0;
    private const int VISIBLE = 1;

    public CrawlerMap()
    {
        fog = new TileMap(); // unrevealed tiles are -1 by default.
    }

    // Assume this is always true.
    // Hopefully ez fix if its an issue.
    public static bool TileIsWall(int id)
    {
        return id == -1 || id == 6;
    }

    // Return value to be sent to ViewModel.
    // Radius should be a small reasonable number, like 6.
    public int[,] GetVisibleTiles((int x, int y) pos, int radius = 6)
    {
        ClearVisibility();
        UpdateVisibility(pos, radius);

        int[,] tiles = new int[radius * 2 + 1, radius * 2 + 1];
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                tiles[dx + radius, dy + radius] = 
                    fog.GetCell(pos.x + dx, pos.y + dy) == VISIBLE ?
                    this.GetCell(pos.x + dx, pos.y + dy) : -2;
                    // -2 and not -1, in case theres a hole in the ground or something
            }
        }
        return tiles;
    }

    private void ClearVisibility()
    {
        foreach (Vector2 vec in fog.GetUsedCellsById(VISIBLE))
        {
            fog.SetCellv(vec, REVEALED);
        }
    }

    // Tiles marked as VISIBLE are not meant to be saved!
    public void UpdateVisibility((int x, int y) pos, int radius)
    {
        // For each unique slope passing through a cell,
        foreach ((int x, int y) in GridHelper.ListRationals(radius))
        {
            // Mark every cell on that slope, for each of the 8 octants.
            MarkLineOfSight((pos.x, pos.y), (pos.x + x, pos.y + y));
            MarkLineOfSight((pos.x, pos.y), (pos.x - x, pos.y + y));
            MarkLineOfSight((pos.x, pos.y), (pos.x + x, pos.y - y));
            MarkLineOfSight((pos.x, pos.y), (pos.x - x, pos.y - y));
            MarkLineOfSight((pos.x, pos.y), (pos.x + y, pos.y + x));
            MarkLineOfSight((pos.x, pos.y), (pos.x - y, pos.y + x));
            MarkLineOfSight((pos.x, pos.y), (pos.x + y, pos.y - x));
            MarkLineOfSight((pos.x, pos.y), (pos.x - y, pos.y - x));
        }
    }

    private void MarkLineOfSight((int x, int y) from, (int x, int y) to)
    {
        foreach ((int x, int y) in GridHelper.LineBetween(from, to))
        {
            fog.SetCell(x, y, VISIBLE);
            if (TileIsWall(this.GetCell(x, y)))
            {
                return;
            }
        }
        return;
    }
}
