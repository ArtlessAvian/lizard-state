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

    }

    public void Run(Model model) // ModelAPI maybe?
    {
        List<Entity> dirty = new List<Entity>();
        foreach (Entity e in model.GetEntities())
        {
            if (lastSeenAt.ContainsKey(e.id))
            {
                if (new Vector2(e.position.x, e.position.y) == lastSeenAt[e.id]) { continue; }
            }
            lastSeenAt[e.id] = new Vector2(e.position.x, e.position.y);
            dirty.Add(e);
        }

        // batch all sees first to avoid thrashing when swapping
        foreach (Entity e in dirty)
        {
            if (e.providesVision)
            {
                SeeUnseen(model, e);
            }
            else
            {
                GetSeen(model, e);
            }

        }
        foreach (Entity e in dirty)
        {
            if (e.providesVision)
            {
                UnseeSeen(model, e);
            }
            else
            {
                GetUnseen(model, e);
            }
        }
    }

    public void RefreshVision(Model model, Entity e)
    {
        if (e.providesVision)
        {
            SeeUnseen(model, e);
            UnseeSeen(model, e);
        }
        else
        {
            GetSeen(model, e);
            GetUnseen(model, e);
        }
    }

    private void SeeUnseen(Model model, Entity e)
    {
        // every entity that /could/ be seen.
        foreach (Entity other in model.GetEntitiesInRadius(e.position, 8))
        {
            bool seeing = VisibilityTrie.AnyLineOfSight(e.position, other.position, x => model.TileBlocksVision(x));
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
                if (!seenBy[other.id].Contains(e.id)) { seenBy[other.id].Add(e.id); }

                if (!canSee.ContainsKey(e.id)) { canSee[e.id] = new List<int>(); }
                if (!canSee[e.id].Contains(other.id)) { canSee[e.id].Add(other.id); };
            }
        }
    }

    private void UnseeSeen(Model model, Entity e)
    {
        // TODO: figure out why this happens.
        if (!canSee.ContainsKey(e.id)) { return; }

        for (int i = canSee[e.id].Count - 1; i >= 0; i--)
        {
            Entity other = model.GetEntity(canSee[e.id][i]);

            bool seeing = GridHelper.Distance(other.position.x - e.position.x, other.position.y - e.position.y) <= 8;
            seeing = seeing && VisibilityTrie.AnyLineOfSight(e.position, other.position, x => model.TileBlocksVision(x));

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

    private void GetSeen(Model model, Entity e)
    {
        foreach (int playerId in canSee.Keys)
        {
            Entity player = model.GetEntity(playerId);

            bool seeing = GridHelper.Distance(e.position.x - player.position.x, e.position.y - player.position.y) <= 8;
            seeing = seeing && VisibilityTrie.AnyLineOfSight(player.position, e.position, x => model.TileBlocksVision(x));

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
                if (!seenBy[e.id].Contains(player.id)) { seenBy[e.id].Add(player.id); }

                // leaving this in to be explicit. never runs.
                if (!canSee.ContainsKey(player.id)) { canSee[player.id] = new List<int>(); }
                if (!canSee[player.id].Contains(e.id)) { canSee[player.id].Add(e.id); }
            }
        }
    }

    private void GetUnseen(Model model, Entity e)
    {
        if (!seenBy.ContainsKey(e.id)) { return; }

        for (int i = seenBy[e.id].Count - 1; i >= 0; i--)
        {
            Entity player = model.GetEntity(seenBy[e.id][i]);

            bool seeing = GridHelper.Distance(e.position.x - player.position.x, e.position.y - player.position.y) <= 8;
            seeing = seeing && VisibilityTrie.AnyLineOfSight(player.position, e.position, x => model.TileBlocksVision(x));

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
}
