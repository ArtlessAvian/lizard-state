using System;
using Godot;
using Godot.Collections;

/// <summary>
/// Stores fog of war and entity vision information.
/// Maybe split these two responsibilities.
/// </summary>
public class FogOfWarSystem : SparseMatrix, CrawlerSystem
{
    [Export] public Dictionary<int, Vector2> lastSeenAt = new Dictionary<int, Vector2>();
    [Export] public Dictionary<int, Vector3[]> lastVision = new Dictionary<int, Vector3[]>();

    private const int REVEALED = 0;
    private const int VISIBLE = 1;

    public void ProcessEvent(Model model, Dictionary ev)
    {

    }

    public void Run(Model model)
    {
        foreach (Entity e in model.GetEntities())
        {
            if (!e.providesVision) { continue; }

            if (lastSeenAt.ContainsKey(e.id))
            {
                if (new Vector2(e.position.x, e.position.y) == lastSeenAt[e.id]) { continue; }
            }

            RefreshVision(model, e);
        }
    }

    public void RefreshVision(Model model, Entity e)
    {
        CrawlerMap map = model.GetMap();

        lastSeenAt[e.id] = new Vector2(e.position.x, e.position.y);
        lastVision[e.id] = this.GetVisibleTiles(map, e.position, 8);

        model.CoolerApiEvent(new Dictionary(){
            {"subject", e.id},
            {"action", "SeeMap"},
            {"tiles", lastVision[e.id]}
        });
    }

    // Return value to be sent to ViewModel.
    // Radius should be a small reasonable number, like 5.
    private Vector3[] GetVisibleTiles(CrawlerMap map, (int x, int y) pos, int radius = 5)
    {
        ClearVisibility();
        UpdateVisibility(map, pos, radius);

        var tiles = new System.Collections.Generic.List<Vector3>();
        for (int y = pos.y - radius; y <= pos.y + radius; y++)
        {
            for (int x = pos.x - radius; x <= pos.x + radius; x++)
            {
                if (this.GetCell(x, y) == VISIBLE)
                {
                    tiles.Add(new Vector3(x, y, map.GetCell(x, y)));
                }
            }
        }
        return tiles.ToArray();
    }

    private void ClearVisibility()
    {
        foreach (Vector2 vec in this.GetUsedCellsById(VISIBLE))
        {
            this.SetCellv(vec, REVEALED);
        }
    }

    // Tiles marked as VISIBLE are not meant to be saved!
    private void UpdateVisibility(CrawlerMap map, (int x, int y) pos, int radius)
    {
        Predicate<(int, int)> IsBlocked = ((int x, int y) rel) => map.TileIsWall((pos.x + rel.x, pos.y + rel.y));

        foreach ((int x, int y) relative in VisibilityTrie.FieldOfView(IsBlocked, radius))
        {
            this.SetCell(pos.x + relative.x, pos.y + relative.y, VISIBLE);
        }
    }
}
