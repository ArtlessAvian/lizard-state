using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// How actions get information from the model.
/// </summary>
// Honestly could be kind of pointless. Maybe just give actions full information about the model?
// I'm not doing any unit testing shenanigans, so Model is the only implementation.
// TODO: Rename ModelQueries? I dunno.
public interface ModelAPI
{
    void ApiEvent(ModelEvent ev);

    // Maybe make MapAPI. (MapQueries?)
    CrawlerMap GetMap();

    Entity GetEntity(int id);
    Entity GetPlayer();
    Entity GetEntityAt((int x, int y) position);
    List<Entity> GetEntitiesInRadius((int x, int y) position, int radius);

    bool CanWalkFromTo((int x, int y) position, (int x, int y) position2);

    // maybe this belongs in some math helper thing or something
    int Distance((int x, int y) position, (int x, int y) position2);
}

public partial class Model : ModelAPI
{
    public void ApiEvent(ModelEvent ev)
    {
        this.NewEvent(ev);
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
            if (Distance(position, e.position) <= radius)
            {
                inRadius.Add(e);
            }
        }
        return inRadius;
    }

    // TODO: Disallow corner cutting?
    public bool CanWalkFromTo((int x, int y) position, (int x, int y) position2)
    {
        return !CrawlerMap.TileIsWall(Map.GetCell(position2.x, position2.y));
    }

    public int Distance((int x, int y) pos, (int x, int y) pos2)
    {
        return Math.Max(Math.Abs(pos.x - pos2.x), Math.Abs(pos.y - pos2.y));
    }
}
