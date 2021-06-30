using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Stores map and vision information.
/// Maybe ask the 
/// </summary>
public class VisionSystem : TileMap
{
    [Export] NodePath mapPath;
    CrawlerMap map = null;

    private const int REVEALED = 0;
    private const int VISIBLE = 1;

    public void Run(Model model) // ModelAPI maybe?
    {
        foreach (Entity e in model.Entities.GetChildren())
        {
            if (e.dirtyVision)
            {
                UpdateVision(model, e);
            }
        }
    }

    public void UpdateVision(Model model, Entity e)
    {
        model.NewEvent(new ModelEvent(e.id, "SeeMap", (e.position, this.GetVisibleTiles(e.position, 5))));
        e.dirtyVision = false;
    }

    // Return value to be sent to ViewModel.
    // Radius should be a small reasonable number, like 6.
    public int[,] GetVisibleTiles((int x, int y) pos, int radius = 6)
    {
        if (map == null) { map = GetNode<CrawlerMap>(mapPath); }

        ClearVisibility();
        UpdateVisibility(pos, radius);

        int[,] tiles = new int[radius * 2 + 1, radius * 2 + 1];
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                tiles[dx + radius, dy + radius] = 
                    this.GetCell(pos.x + dx, pos.y + dy) == VISIBLE ?
                    map.GetCell(pos.x + dx, pos.y + dy) : -2;
                    // -2 and not -1, in case theres a hole in the ground or something
            }
        }
        return tiles;
    }

    private void ClearVisibility()
    {
        foreach (Vector2 vec in this.GetUsedCellsById(VISIBLE))
        {
            this.SetCellv(vec, REVEALED);
        }
    }

    // Tiles marked as VISIBLE are not meant to be saved!
    public void UpdateVisibility((int x, int y) pos, int radius)
    {
        // For each unique slope passing through a cell,
        foreach ((int x, int y) in GridHelper.ListRationals(radius))
        {
            // Experiment with diagonal stuff. This makes a big octagon.
            // int scale = (int)(radius / (y + 0.5 * x)); // Compromise
            // int scale = (int)(radius / (y + x)); // Taxicab 
            int scale = radius / y; // Chebyshev

            // Mark every cell on that slope, for each of the 8 octants.
            MarkLineOfSight((pos.x, pos.y), (pos.x + x * scale, pos.y + y * scale));
            MarkLineOfSight((pos.x, pos.y), (pos.x - x * scale, pos.y + y * scale));
            MarkLineOfSight((pos.x, pos.y), (pos.x + x * scale, pos.y - y * scale));
            MarkLineOfSight((pos.x, pos.y), (pos.x - x * scale, pos.y - y * scale));
            MarkLineOfSight((pos.x, pos.y), (pos.x + y * scale, pos.y + x * scale));
            MarkLineOfSight((pos.x, pos.y), (pos.x - y * scale, pos.y + x * scale));
            MarkLineOfSight((pos.x, pos.y), (pos.x + y * scale, pos.y - x * scale));
            MarkLineOfSight((pos.x, pos.y), (pos.x - y * scale, pos.y - x * scale));
        }
    }

    private void MarkLineOfSight((int x, int y) from, (int x, int y) to)
    {
        foreach ((int x, int y) in GridHelper.LineBetween(from, to))
        {
            this.SetCell(x, y, VISIBLE);
            if (CrawlerMap.TileIsWall(map.GetCell(x, y)))
            {
                return;
            }
        }
        return;
    }
}
