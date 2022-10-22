using System;
using Godot;
using System.Collections.Generic;
using GodotDict = Godot.Collections.Dictionary;

/// <summary>
/// Stores fog of war and entity vision information.
/// Maybe split these two responsibilities.
/// </summary>
public class VisionSystem : Resource, CrawlerSystem
{
    // kind of like a dirty flag
    [Export] public Dictionary<int, Vector2> lastSeenAt = new Dictionary<int, Vector2>();

    // ideally these would be hashsets. i think godot can serialize IEnumerables. 
    // alternatively, maybe a bool[seers, entities] could be good too.
    // both counts might change over time though so big rip.
    [Export] public Dictionary<int, List<int>> canSee = new Dictionary<int, List<int>>();
    [Export] public Dictionary<int, List<int>> seenBy = new Dictionary<int, List<int>>();

    public void ProcessEvent(Model model, GodotDict ev)
    {
        // if ((string)ev["action"] != "Move")
        // {
        //     return;
        // }

        // Entity subject = model.GetEntity((int)ev["subject"]);
        // if (!subject.providesVision) { return; }

        // if (lastSeenAt.ContainsKey(subject.id))
        // {
        //     if (HashPosition(subject.position) == lastSeenAt[subject.id]) { return; }
        // }

        // lastSeenAt.Add(subject.id, HashPosition(subject.position));
        // UpdateVision(model, subject);
    }

    public void Run(Model model) // ModelAPI maybe?
    {
        foreach (Entity e in model.GetEntities())
        {
            if (lastSeenAt.ContainsKey(e.id))
            {
                if (new Vector2(e.position.x, e.position.y) == lastSeenAt[e.id]) { continue; }
            }
            lastSeenAt[e.id] = new Vector2(e.position.x, e.position.y);

            if (e.providesVision)
            {
                RefreshVision(model, e);
            }
            else
            {
                RefreshOtherVision(model, e);
            }
        }
    }

    public void RefreshVision(Model model, Entity e)
    {
        CrawlerMap map = model.GetMap();
        Predicate<(int, int)> IsBlocked = ((int x, int y) rel) => map.TileIsWall((e.position.x + rel.x, e.position.y + rel.y));

        // do this first to prevent thrashing the player unsees someone, but the partner sees them after.
        SeeUnseen(model, e, IsBlocked);
        UnseeSeen(model, e, IsBlocked);
    }

    private void SeeUnseen(Model model, Entity e, Predicate<(int, int)> IsBlocked)
    {
        // every entity that /could/ be seen.
        foreach (Entity other in model.GetEntitiesInRadius(e.position, 8))
        {
            bool seeing = VisibilityTrie.AnyLineOfSight((other.position.x - e.position.x, other.position.y - e.position.y), IsBlocked);
            if (seeing)
            {
                if (!seenBy.ContainsKey(other.id))
                {
                    seenBy[other.id] = new List<int>();
                }
                if (seenBy[other.id].Count == 0)
                {
                    other.visibleToPlayer = true;
                    model.CoolerApiEvent(e.id, "See", null, other.id);
                }
                seenBy[other.id].Add(e.id);

                if (!canSee.ContainsKey(e.id)) { canSee[e.id] = new List<int>(); }
                canSee[e.id].Add(other.id);
            }
        }
    }

    private void UnseeSeen(Model model, Entity e, Predicate<(int, int)> IsBlocked)
    {
        // foreach (int otherId in canSee[e.id])
        // foreach (int otherId in canSee[e.id])
        for (int i = canSee[e.id].Count - 1; i >= 0; i--)
        {
            Entity other = model.GetEntity(canSee[e.id][i]);

            int dx = other.position.x - e.position.x;
            int dy = other.position.y - e.position.y;
            bool seeing = GridHelper.Distance(dx, dy) <= 8;
            seeing = seeing && VisibilityTrie.AnyLineOfSight((dx, dy), IsBlocked);
            if (!seeing)
            {
                seenBy[other.id].Remove(e.id);
                canSee[e.id].Remove(other.id);
                if (seenBy[other.id].Count == 0)
                {
                    other.visibleToPlayer = false;
                    model.CoolerApiEvent(other.id, "Unsee");
                }
            }
        }
    }

    private void RefreshOtherVision(Model model, Entity e)
    {
        GetSeen(model, e);
        GetUnseen(model, e);
    }

    private void GetSeen(Model model, Entity e)
    {
        foreach (int playerId in canSee.Keys)
        {
            Entity player = model.GetEntity(playerId);

            int dx = e.position.x - player.position.x;
            int dy = e.position.y - player.position.y;
            bool seeing = GridHelper.Distance(dx, dy) <= 8;

            CrawlerMap map = model.GetMap();
            Predicate<(int, int)> IsBlocked = ((int x, int y) rel) => map.TileIsWall((player.position.x + rel.x, player.position.y + rel.y));
            seeing = seeing && VisibilityTrie.AnyLineOfSight((dx, dy), IsBlocked);

            if (seeing)
            {
                if (!seenBy.ContainsKey(e.id))
                {
                    seenBy[e.id] = new List<int>();
                }
                if (seenBy[e.id].Count == 0)
                {
                    e.visibleToPlayer = true;
                    model.CoolerApiEvent(player.id, "See", null, e.id);
                }
                seenBy[e.id].Add(player.id);

                // leaving this in to be explicit. never runs.
                if (!canSee.ContainsKey(player.id)) { canSee[player.id] = new List<int>(); }
                canSee[player.id].Add(e.id);
            }
        }
    }

    private void GetUnseen(Model model, Entity e)
    {
        if (!seenBy.ContainsKey(e.id)) { return; }

        for (int i = seenBy[e.id].Count - 1; i >= 0; i--)
        {
            Entity player = model.GetEntity(seenBy[e.id][i]);

            int dx = e.position.x - player.position.x;
            int dy = e.position.y - player.position.y;
            bool seeing = GridHelper.Distance(dx, dy) <= 8;

            CrawlerMap map = model.GetMap();
            Predicate<(int, int)> IsBlocked = ((int x, int y) rel) => map.TileIsWall((player.position.x + rel.x, player.position.y + rel.y));
            seeing = seeing && VisibilityTrie.AnyLineOfSight((dx, dy), IsBlocked);
            if (!seeing)
            {
                seenBy[e.id].Remove(player.id);
                canSee[player.id].Remove(e.id);
                if (seenBy[e.id].Count == 0)
                {
                    e.visibleToPlayer = false;
                    model.CoolerApiEvent(e.id, "Unsee");
                }
            }
        }
    }

    private Predicate<(int, int)> GetIsBlockedPredicate(CrawlerMap map, Entity e)
    {
        return ((int x, int y) rel) => map.TileIsWall((e.position.x + rel.x, e.position.y + rel.y));
    }

    private Predicate<(Entity seer, Entity target)> GetCanSeePredicate(Predicate<(int, int)> IsBlocked)
    {
        return ((Entity seer, Entity target) pair) =>
            GridHelper.Distance(pair.target.position.x - pair.seer.position.x, pair.target.position.y - pair.seer.position.y) <= 8 &&
            VisibilityTrie.AnyLineOfSight((pair.target.position.x - pair.seer.position.x, pair.target.position.y - pair.seer.position.y), IsBlocked);
    }
}
