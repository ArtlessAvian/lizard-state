using System;
using Godot;
using Godot.Collections;

/// <summary>
/// Stores fog of war and entity vision information.
/// Maybe split these two responsibilities.
/// </summary>
public class FogOfWarSystem : TileMap, CrawlerSystem
{
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

    public void Run(Model model)
    {
        foreach (Entity e in model.GetEntities())
        {
            if (e.providesVision)
            {
                UpdateVision(model, e);
                e.dirtyVision = false;
            }
        }
    }

    public void UpdateVision(Model model, Entity e)
    {
        CrawlerMap map = model.GetMap();

        // See the map
        int[,] tiles = this.GetVisibleTiles(map, e.position, 8);

        model.CoolerApiEvent(new Dictionary(){
            {"subject", e.id},
            {"action", "SeeMap"},
            {"center", new Vector2(e.position.x, e.position.y)},
            {"tiles", (tiles)}
        });
    }

    // Return value to be sent to ViewModel.
    // Radius should be a small reasonable number, like 5.
    public int[,] GetVisibleTiles(CrawlerMap map, (int x, int y) pos, int radius = 5)
    {
        ClearVisibility();
        UpdateVisibility(map, pos, radius);

        int[,] tiles = new int[radius * 2 + 1, radius * 2 + 1];
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                tiles[dx + radius, dy + radius] =
                    this.GetCell(pos.x + dx, pos.y + dy) == VISIBLE ?
                    map.GetCell(pos.x + dx, pos.y + dy) : -2;
                // -2 and not -1, in case theres a hole in the ground or something

                // map.GetCell(pos.x + dx, pos.y + dy);
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

    // // Tiles marked as VISIBLE are not meant to be saved!
    public void UpdateVisibility(CrawlerMap map, (int x, int y) pos, int radius)
    {
        Predicate<(int, int)> isBlocked = ((int x, int y) rel) => map.TileIsWall((pos.x + rel.x, pos.y + rel.y));

        foreach ((int x, int y) relative in VisibilityTrie.FieldOfView(isBlocked, radius))
        {
            this.SetCell(pos.x + relative.x, pos.y + relative.y, VISIBLE);
        }
    }
}
