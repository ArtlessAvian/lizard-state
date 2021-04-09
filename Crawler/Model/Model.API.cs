using Godot;
using System;
using System.Collections.Generic;

public interface ModelAPI
{
    void NewEvent(ModelEvent ev);

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
    public void NewEvent(ModelEvent ev)
    {
        eventQueue.Add(ev);
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

    public List<Entity> GetEntitiesInRadius((int x, int y) position, int radius)
    {
        List<Entity> inRadius = new List<Entity>();
        foreach (Entity e in entities)
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
        return !Map.TileIsWall(map.map.GetCell(position2.x, position2.y));
    }

    public int Distance((int x, int y) pos, (int x, int y) pos2)
    {
        return Math.Max(Math.Abs(pos.x - pos2.x), Math.Abs(pos.y - pos2.y));
    }
}
