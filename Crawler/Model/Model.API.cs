using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// How actions get information from the model.
/// </summary>
// TODO: Make "How non-model classes get information from the model."
// All model-y classes can just be passed the model.
// Some of these queries are important tho.
public interface ModelAPI
{
    void CoolerApiEvent(int subject, string action, object args = null, int @object = -1);
    void CoolerApiEvent(Godot.Collections.Dictionary @event);

    // Maybe make MapAPI. (MapQueries?)
    CrawlerMap GetMap();

    Entity GetEntity(int id);
    Entity GetPlayer();
    Entity GetEntityAt((int x, int y) position);
    List<Entity> GetEntitiesInRadius((int x, int y) position, int radius);
    List<Entity> GetEntitiesInSight(int team);

    bool CanWalkFromTo((int x, int y) position, (int x, int y) position2);
}

public partial class Model : ModelAPI
{
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
        // TODO: Remove compatibility
        if (!@event.Contains("subject")) {@event.Add("subject", -1);}
        if (!@event.Contains("args")) {@event.Add("args", null);}
        if (!@event.Contains("object")) {@event.Add("object", -1);}

        // For each system, handle/decorate the event.
        foreach (CrawlerSystem system in GetNode("Systems").GetChildren())
        {
            system.ProcessEvent(this, @event);
        }
        // Send the event to the view, if the player('s team) sees it.

        // this.EmitSignal("")
        this.NewEvent(@event);
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

    public List<Entity> GetEntitiesInRadius((int x, int y) position, int radius)
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
        return !CrawlerMap.TileIsWall(Map.GetCell(position2.x, position2.y)) &&
                !CrawlerMap.TileIsWall(Map.GetCell(position.x, position.y));
    }

    public int Distance((int x, int y) pos, (int x, int y) pos2)
    {
        return GridHelper.Distance(pos, pos2);
        // return Math.Max(Math.Abs(pos.x - pos2.x), Math.Abs(pos.y - pos2.y));
    }
}
