using System.Collections.Generic;
using Godot;
using GDDict = Godot.Collections.Dictionary;
using LizardState.Engine;

/// <summary>
/// Stores fog of war and entity vision information.
/// Maybe split these two responsibilities.
/// </summary>
public class FogOfWarSystem : Resource, CrawlerSystem
{
    [Export] private SparseMatrix revealStatus = new SparseMatrix();
    [Export] private Dictionary<int, Vector2> lastSeenAt = new Dictionary<int, Vector2>();
    [Export] public Dictionary<int, Vector3[]> lastVision = new Dictionary<int, Vector3[]>();

    public const int UNREVEALED = -1;
    public const int REVEALED = 0;
    public const int VISIBLE = 1;

    public void ProcessEvent(Model model, GDDict ev)
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

        model.CoolerApiEvent(new GDDict(){
            {"subject", e.id},
            {"action", "SeeMap"},
            {"tiles", lastVision[e.id]}
        });
    }

    // Return value to be sent to ViewModel.
    // Radius should be a small reasonable number, like 5.
    private Vector3[] GetVisibleTiles(CrawlerMap map, AbsolutePosition pos, int radius = 5)
    {
        ClearVisibility();
        UpdateVisibility(map, pos, radius);

        var tiles = new System.Collections.Generic.List<Vector3>();
        for (int y = pos.y - radius; y <= pos.y + radius; y++)
        {
            for (int x = pos.x - radius; x <= pos.x + radius; x++)
            {
                if (revealStatus.GetCell(x, y) == VISIBLE)
                {
                    tiles.Add(new Vector3(x, y, map.tiles.GetCell(x, y)));
                }
            }
        }
        return tiles.ToArray();
    }

    private void ClearVisibility()
    {
        foreach (Vector2 vec in revealStatus.GetUsedCellsById(VISIBLE))
        {
            revealStatus.SetCellv(vec, REVEALED);
        }
    }

    // Tiles marked as VISIBLE are not meant to be saved!
    private void UpdateVisibility(CrawlerMap map, AbsolutePosition pos, int radius)
    {
        foreach (AbsolutePosition tile in VisibilityTrie.FieldOfView(pos, x => map.TileIsWall(x), radius))
        {
            revealStatus.SetCell(tile.x, tile.y, VISIBLE);
        }
    }

    public int GetStatus(AbsolutePosition pos)
    {
        return revealStatus.GetCell(pos.x, pos.y);
    }

    public IEnumerable<AbsolutePosition> GetRevealed()
    {
        return revealStatus.GetUsedCellsIterator();
    }
}
