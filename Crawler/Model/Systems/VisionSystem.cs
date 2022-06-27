using System;
using Godot;
using Godot.Collections;

/// <summary>
/// Stores fog of war and entity vision information.
/// Maybe split these two responsibilities.
/// </summary>
public class VisionSystem : Node, CrawlerSystem
{
    [Export] public Dictionary<int, int> lastSeenAt = new Dictionary<int, int>();
    [Export] public Dictionary<int, int> canSee = new Dictionary<int, int>();

    public void ProcessEvent(Model model, Dictionary ev)
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
            if (!e.providesVision) { continue; }

            if (lastSeenAt.ContainsKey(e.id))
            {
                if (HashPosition(e.position) == lastSeenAt[e.id]) { continue; }
            }

            lastSeenAt[e.id] = HashPosition(e.position);
            UpdateVision(model, e);
        }
    }

    public void UpdateVision(Model model, Entity e)
    {
        CrawlerMap map = model.GetMap();
        Predicate<(int, int)> IsBlocked = ((int x, int y) pos) => CrawlerMap.TileIsWall(map.GetCell(pos.x, pos.y));

        UnseeSeen(model, e, IsBlocked);
        SeeUnseen(model, e, IsBlocked);
    }

    private void SeeUnseen(Model model, Entity e, Predicate<(int, int)> IsBlocked)
    {
        foreach (Entity other in model.GetEntitiesInRadius(e.position, 8))
        {
            // don't try to be smart. just draw the ray.
            // bool seeing = GetCell(other.position.x, other.position.y) == 1;
            bool seeing = VisibilityTrie.AnyLineOfSight((other.position.x - e.position.x, other.position.y - e.position.y), IsBlocked);

            if (seeing)
            {
                if (!canSee.ContainsKey(other.id))
                {
                    canSee[other.id] = 0;
                }
                if (canSee[other.id] == 0)
                {
                    other.visibleToPlayer = true;
                    model.CoolerApiEvent(e.id, "See", null, other.id);
                    model.CoolerApiEvent(-1, "Wait");
                }
                canSee[other.id] |= 1 << e.id;
            }
        }
    }

    private void UnseeSeen(Model model, Entity e, Predicate<(int, int)> IsBlocked)
    {
        foreach (Entity other in model.GetEntities())
        {
            if (canSee.ContainsKey(other.id) && (canSee[other.id] & (1 << e.id)) != 0)
            {
                int dx = other.position.x - e.position.x;
                int dy = other.position.y - e.position.y;
                bool seeing = GridHelper.Distance(dx, dy) <= 8;
                seeing = seeing && VisibilityTrie.AnyLineOfSight((dx, dy), IsBlocked);
                if (!seeing)
                {
                    canSee[other.id] &= ~(1 << e.id);
                    if (canSee[other.id] == 0)
                    {
                        other.visibleToPlayer = false;
                        canSee.Remove(other.id);
                        model.CoolerApiEvent(other.id, "Unsee");
                    }
                }
            }
        }
    }

    // for this to return the same, thing, you have to move very specifically and weirdly.
    private static int HashPosition((int x, int y) position)
    {
        return position.x * 10 + position.y * 30;
    }
}
