using System;
using System.Collections.Generic;
using Godot;

public partial class Model
{
    public CrawlerMap GetMap()
    {
        return this.map;
    }

    // Really, really slow.
    public T GetSystem<T>() where T : CrawlerSystem
    {
        foreach (Resource sys in systems)
        {
            if (sys is T systemCast) { return systemCast; }
        }
        return default(T);
    }

    public List<Entity> GetEntities()
    {
        return entities;
    }

    public List<FloorItem> GetFloorItems()
    {
        return floorItems;
    }

    public Entity GetEntity(int id)
    {
        return entities[id];
    }

    public Entity GetPlayer()
    {
        return GetEntity(0);
    }

    // There is one unique OK/STUN entity at every position.
    public Entity GetEntityAt((int x, int y) position)
    {
        foreach (Entity e in GetEntities())
        {
            // rip no tuple equality
            if (e.position.x == position.x && e.position.y == position.y)
            {
                if (e.state == Entity.EntityState.OK || e.state == Entity.EntityState.STUN)
                {
                    return e;
                }
            }
        }
        return null;
    }

    public List<Entity> GetEntitiesInRadius((int x, int y) position, float radius)
    {
        List<Entity> inRadius = new List<Entity>();
        foreach (Entity e in GetEntities())
        {
            if (Distance(position, e.position) <= radius)
            {
                if (e.state == Entity.EntityState.OK || e.state == Entity.EntityState.STUN)
                {
                    inRadius.Add(e);
                }
            }
        }
        return inRadius;
    }

    public List<Entity> GetEntitiesInLOS((int x, int y) position, float radius)
    {
        List<Entity> inSight = GetEntitiesInRadius(position, radius);

        Predicate<(int, int)> isBlocked = ((int x, int y) rel) => GetMap().TileIsWall((position.x + rel.x, position.y + rel.y));

        for (int i = inSight.Count - 1; i >= 0; i--)
        {
            (int x, int y) relative = (inSight[i].position.x - position.x, inSight[i].position.y - position.y);
            if (!VisibilityTrie.AnyLineOfSight(relative, isBlocked))
            {
                inSight.RemoveAt(i);
            }
        }
        return inSight;
    }

    public List<Entity> GetEntitiesInCone((int x, int y) position, float radius, (int x, int y) direction, float sectorDegrees)
    {
        // Not good code reuse. Copy-pasted from LOS.
        VisionSystem vision = GetSystem<VisionSystem>();
        // vision.trie.ExtendRadius(radius);

        List<Entity> inCone = GetEntitiesInRadius(position, radius);

        Predicate<(int, int)> notInCone = ((int x, int y) rel) => !VisibilityTrie.TileInCone(rel, direction, sectorDegrees);
        Predicate<(int, int)> isBlocked = ((int x, int y) rel) => GetMap().TileIsWall((position.x + rel.x, position.y + rel.y));
        Predicate<(int, int)> isEither = ((int, int) rel) => notInCone(rel) || isBlocked(rel);

        for (int i = inCone.Count - 1; i >= 0; i--)
        {
            (int x, int y) relative = (inCone[i].position.x - position.x, inCone[i].position.y - position.y);
            if (!VisibilityTrie.AnyLineOfSight(relative, isEither))
            {
                inCone.RemoveAt(i);
            }
        }
        return inCone;
    }

    public List<Entity> GetEntitiesInSight(int team)
    {
        if (team == 0)
        {
            VisionSystem visionSystem = GetSystem<VisionSystem>();
            List<Entity> entities = new List<Entity>();
            foreach (List<int> list in visionSystem.canSee.Values)
            {
                foreach (int id in list)
                {
                    Entity seen = GetEntity(id);
                    if (!entities.Contains(seen)) { entities.Add(seen); }
                }
            }
            return entities;
        }
        else
        {
            // Just find check distance and line of sight directly.
            // TODO: Write it.
            return new List<Entity> { };
        }
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
