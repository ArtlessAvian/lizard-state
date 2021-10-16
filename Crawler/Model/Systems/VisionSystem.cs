using System;
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

    public VisibilityTrie trie = new VisibilityTrie();

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
        foreach (Entity e in model.GetEntities())
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
        int[,] tiles = this.GetVisibleTiles(e.position, 8);

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
                    model.CoolerApiEvent(-1, "Wait");
                }
                canSee[other.id] |= 1 << e.id;
            }
        }
        
        // Unsee things you already see.
        foreach (Entity other in model.GetEntities())
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
    public int[,] GetVisibleTiles((int x, int y) pos, int radius = 5)
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
    public void UpdateVisibility((int x, int y) pos, int radius)
    {
        // trie.ExtendRadius(radius);
        
        Predicate<(int, int)> isBlocked = ((int x, int y) rel) => map.TileIsWall((pos.x + rel.x, pos.y + rel.y));

        foreach ((int x, int y) relative in VisibilityTrie.FieldOfView(isBlocked, radius))
        {
            this.SetCell(pos.x + relative.x, pos.y + relative.y, VISIBLE);
        }
        
        // Traverse the trie 8 times. Could be done in one pass.
        
        // for (int octant = 0; octant < 8; octant++)
        // {
        //     FollowTrie(pos, trie.origin, octant, radius);
        // }
    }

    // private void FollowTrie((int x, int y) origin, VisibilityTrie.TrieNode node, int octant, int radius)
    // {
    //     if (node == null) {return;}

    //     if (GridHelper.Distance(node.x, node.y) > radius)
    //     {
    //         return;
    //     }
        
    //     (int dx, int dy) = GridHelper.DeOctantify(node.x, node.y, octant);

    //     this.SetCell(origin.x + dx, origin.y + dy, VISIBLE);

    //     if (!map.TileIsWall((origin.x + dx, origin.y + dy)))
    //     {
    //         FollowTrie(origin, node.straight, octant, radius);
    //         FollowTrie(origin, node.diag, octant, radius);
    //     }
    // }



    // public void UpdateVisibility((int x, int y) pos, int radius)
    // {
    //     // skip visibility check, for speed comparison.
    //     // TODO: (half the time is spent on vision checks.)
    //     // for (int y = pos.y - radius; y <= pos.y + radius; y++)
    //     // {
    //     //     for (int x = pos.x - radius; x <= pos.x + radius; x++)
    //     //     {
    //     //         this.SetCell(x, y, VISIBLE);
    //     //     }
    //     // }
    //     // return;

    //     // For each unique slope passing through a cell,
    //     foreach ((int rise, int run) in GridHelper.ListRationals(radius))
    //     {
    //         int thing = radius * rise / run;

    //         // Mark every cell on that slope, for each of the 8 octants.
    //         MarkLineOfSight((pos.x, pos.y), (pos.x + radius, pos.y + thing), radius);
    //         MarkLineOfSight((pos.x, pos.y), (pos.x - radius, pos.y + thing), radius);
    //         MarkLineOfSight((pos.x, pos.y), (pos.x + radius, pos.y - thing), radius);
    //         MarkLineOfSight((pos.x, pos.y), (pos.x - radius, pos.y - thing), radius);
    //         MarkLineOfSight((pos.x, pos.y), (pos.x + thing, pos.y + radius), radius);
    //         MarkLineOfSight((pos.x, pos.y), (pos.x - thing, pos.y + radius), radius);
    //         MarkLineOfSight((pos.x, pos.y), (pos.x + thing, pos.y - radius), radius);
    //         MarkLineOfSight((pos.x, pos.y), (pos.x - thing, pos.y - radius), radius);
    //     }
    // }

    // private void MarkLineOfSight((int x, int y) from, (int x, int y) to, int radius)
    // {
    //     foreach ((int x, int y) in GridHelper.LineBetween(from, to))
    //     {
    //         if (GridHelper.Distance(from, (x, y)) > radius)
    //         {
    //             return;
    //         }

    //         this.SetCell(x, y, VISIBLE);
    //         if (map.TileIsWall((x, y)))
    //         {
    //             return;
    //         }
    //     }
    //     return;
    // }
}
