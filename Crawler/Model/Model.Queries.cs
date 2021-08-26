using Godot;
using System;
using System.Collections.Generic;

public partial class Model
{
    // todo: rename this lmao
    // also move out of the "queries" file.
    // [Obsolete]
    public void CoolerApiEvent(int subject, string action, object args = null, int @object = -1)
    {
        CoolerApiEvent(new Godot.Collections.Dictionary()
        {
            {"subject", subject},
            {"action", action},
            {"args", args},
            {"object", @object}
        });
    }

    public void CoolerApiEvent(Godot.Collections.Dictionary @event)
    {
        @event.Add("timestamp", time);

        // For each system, decorate the event.
        foreach (CrawlerSystem system in GetNode("Systems").GetChildren())
        {
            system.ProcessEvent(this, @event);
        }

        // Send the event to the view, if the player('s team) sees it.
        this.NewEvent(@event);

        // For each system, react to the event.
        // (Skill procs, or something? could be fun)
    }

    public CrawlerMap GetMap()
    {
        return this.Map;
    }

    public Entity GetEntity(int id)
    {
        return (Entity)Entities.GetChild(id);
    }

    public Entity GetPlayer()
    {
        return GetEntity(0);
    }

    public Entity GetEntityAt((int x, int y) position)
    {
        foreach (Entity e in Entities.GetChildren())
        {
            // rip no tuple equality
            if (e.position.x == position.x && e.position.y == position.y && !e.downed)
            {
                return e;
            }
        }
        return null;
    }

    public List<Entity> GetEntitiesInRadius((int x, int y) position, float radius)
    {
        List<Entity> inRadius = new List<Entity>();
        foreach (Entity e in Entities.GetChildren())
        {
            if (Distance(position, e.position) <= radius && !e.downed)
            {
                inRadius.Add(e);
            }
        }
        return inRadius;
    }

    public List<Entity> GetEntitiesInSight(int team)
    {
        if (team == 0)
        {
            VisionSystem visionSystem = GetNode<VisionSystem>("Systems/Vision");
            List<Entity> entities = new List<Entity>();
            foreach (int i in visionSystem.canSee.Keys)
            {
                entities.Add(GetEntity(i));
            }
            return entities;
        }
        else
        {
            // Just find check distance and line of sight directly.
            // TODO: Write it.
            return new List<Entity>{};
        }
    }

    // TODO: Disallow corner cutting?
    // Should be symmetric. f(x, y) = f(y, x).
    public bool CanWalkFromTo((int x, int y) position, (int x, int y) position2)
    {
        return !Map.TileIsWall((position2.x, position2.y)) &&
                !Map.TileIsWall((position.x, position.y));
    }

    public float Distance((int x, int y) pos, (int x, int y) pos2)
    {
        return GridHelper.Distance(pos, pos2);
        // return Math.Max(Math.Abs(pos.x - pos2.x), Math.Abs(pos.y - pos2.y));
    }
}
