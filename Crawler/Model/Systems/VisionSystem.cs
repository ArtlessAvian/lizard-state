using Godot;
using Godot.Collections;

/// <summary>
/// Stores map and vision information.
/// Maybe ask the 
/// </summary>
public class VisionSystem : TileMap, CrawlerSystem
{
    [Export] NodePath mapPath;
    CrawlerMap map = null;

    public Dictionary<int, int> canSee = new Dictionary<int, int>();

    private const int REVEALED = 0;
    private const int VISIBLE = 1;

    public void ProcessEvent(Model model, Dictionary ev)
    {
        if ((string)ev["action"] == "Move")
        {
            Entity subject = model.GetEntity((int)ev["subject"]);
            if (subject.team == 0)
            {
                UpdateVision(model, subject);
                subject.dirtyVision = false;
            }
        }
    }

    public void Run(Model model) // ModelAPI maybe?
    {
        foreach (Entity e in model.Entities.GetChildren())
        {
            if (e.dirtyVision)
            {
                UpdateVision(model, e);
                e.dirtyVision = false;
            }
        }
    }

    public void UpdateVision(Model model, Entity e)
    {        
        // See the map
        int[,] tiles = this.GetVisibleTiles(e.position, 10);

        model.CoolerApiEvent(new Dictionary(){
            {"subject", e.id},
            {"action", "SeeMap"},
            {"center", new Vector2(e.position.x, e.position.y)},
            {"tiles", (tiles)}
        });
        
        // Update entities seen.
        // See things you don't already see.
        foreach (Entity other in model.GetEntitiesInRadius(e.position, 10))
        {
            bool seeing = GetCell(other.position.x, other.position.y) == 1;
            if (seeing)
            {   
                if (!canSee.ContainsKey(other.id))
                {
                    canSee[other.id] = 0;
                }
                if (canSee[other.id] == 0)
                {
                    model.CoolerApiEvent(e.id, "See", null, other.id);
                }
                canSee[other.id] |= 1 << e.id;
            }
        }
        
        // Unsee things you already see.
        foreach (Entity other in model.Entities.GetChildren())
        {
            if (canSee.ContainsKey(other.id) && (canSee[other.id] & (1 << e.id)) != 0)
            {
                bool seeing = GetCell(other.position.x, other.position.y) == 1;
                if (!seeing)
                {
                    canSee[other.id] &= ~(1 << e.id);
                    if (canSee[other.id] == 0)
                    {
                        canSee.Remove(other.id);
                        model.CoolerApiEvent(other.id, "Unsee");
                    }
                }
            }
        }
    }

    // Return value to be sent to ViewModel.
    // Radius should be a small reasonable number, like 5.
    public int[,] GetVisibleTiles((int x, int y) pos, int radius = 10)
    {
        if (map == null) { map = GetNode<CrawlerMap>(mapPath); }

        ClearVisibility();
        UpdateVisibility(pos, radius);

        int tileRadius = radius / 2;

        int[,] tiles = new int[tileRadius * 2 + 1, tileRadius * 2 + 1];
        for (int dy = -tileRadius; dy <= tileRadius; dy++)
        {
            for (int dx = -tileRadius; dx <= tileRadius; dx++)
            {
                tiles[dx + tileRadius, dy + tileRadius] = 
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
        int tileRadius = radius / 2; // integer division.
        // For each unique slope passing through a cell,
        foreach ((int rise, int run) in GridHelper.ListRationals(tileRadius))
        {
            int thing = tileRadius * rise / run;

            // Mark every cell on that slope, for each of the 8 octants.
            MarkLineOfSight((pos.x, pos.y), (pos.x + tileRadius, pos.y + thing), radius);
            MarkLineOfSight((pos.x, pos.y), (pos.x - tileRadius, pos.y + thing), radius);
            MarkLineOfSight((pos.x, pos.y), (pos.x + tileRadius, pos.y - thing), radius);
            MarkLineOfSight((pos.x, pos.y), (pos.x - tileRadius, pos.y - thing), radius);
            MarkLineOfSight((pos.x, pos.y), (pos.x + thing, pos.y + tileRadius), radius);
            MarkLineOfSight((pos.x, pos.y), (pos.x - thing, pos.y + tileRadius), radius);
            MarkLineOfSight((pos.x, pos.y), (pos.x + thing, pos.y - tileRadius), radius);
            MarkLineOfSight((pos.x, pos.y), (pos.x - thing, pos.y - tileRadius), radius);
        }
    }

    private void MarkLineOfSight((int x, int y) from, (int x, int y) to, int radius)
    {
        foreach ((int x, int y) in GridHelper.LineBetween(from, to))
        {
            if (GridHelper.Distance(from, (x, y)) > radius)
            {
                return;
            }

            this.SetCell(x, y, VISIBLE);
            if (CrawlerMap.TileIsWall(map.GetCell(x, y)))
            {
                return;
            }
        }
        return;
    }
}
