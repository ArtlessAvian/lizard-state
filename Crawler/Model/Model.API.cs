using Godot;
using System;
using System.Collections.Generic;

public interface ModelAPI
{
    void NewEvent(ModelEvent ev);

    Entity GetEntity(int id);
    Entity GetPlayer();
    Entity GetEntityAt(int x, int y);
    List<Entity> GetEntitiesInRadius(int x, int y, int radius);

    bool CanWalkFromTo(int x, int y, int x2, int y2);
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

    public Entity GetEntityAt(int x, int y)
    {
        foreach (Entity e in entities)
        {
            if (e.position.x == x && e.position.y == y && !e.downed)
            {
                return e;
            }
        }
        return null;
    }

    public List<Entity> GetEntitiesInRadius(int x, int y, int radius)
    {
        List<Entity> inRadius = new List<Entity>();
        foreach (Entity e in entities)
        {
            if (Math.Abs(e.position.x - x) <= radius && Math.Abs(e.position.y - y) <= radius && !e.downed)
            {
                inRadius.Add(e);
            }
        }
        return inRadius;
    }

    public bool CanWalkFromTo(int x, int y, int x2, int y2)
    {
        // HACK: huuugeee.
        return !Map.TileIsWall(map.map.GetCell(x2, y2));
    }
}
