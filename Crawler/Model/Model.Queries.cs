using Godot;
using System;
using System.Collections.Generic;

public partial class Model
{
    public CrawlerMap GetMap()
    {
        return this.map;
    }

    public Entity GetEntity(int id)
    {
        return entities[id];
    }

    public Entity GetPlayer()
    {
        return GetEntity(0);
    }

    public Entity GetEntityAt((int x, int y) position)
    {
        foreach (Entity e in entities)
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
        foreach (Entity e in entities)
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
            VisionSystem visionSystem = GetSystem<VisionSystem>();
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

    public T GetSystem<T>()
    {
        foreach (CrawlerSystem system in systems)
        {
            if (system is T systemCasted) { return systemCasted; }
        }
        return default(T); // TODO: figure out if this is null.
    }

    // TODO: Disallow corner cutting?
    // Should be symmetric. f(x, y) = f(y, x).
    public bool CanWalkFromTo((int x, int y) position, (int x, int y) position2)
    {
        return !map.TileIsWall((position2.x, position2.y)) &&
                !map.TileIsWall((position.x, position.y));
    }

    public float Distance((int x, int y) pos, (int x, int y) pos2)
    {
        return GridHelper.Distance(pos, pos2);
        // return Math.Max(Math.Abs(pos.x - pos2.x), Math.Abs(pos.y - pos2.y));
    }
}
