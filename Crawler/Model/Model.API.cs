using Godot;
using System;
using System.Collections.Generic;

public interface ModelAPI
{
    Entity GetEntity(int id);
    Entity GetPlayer();
    Entity GetEntityAt(int x, int y);
    List<Entity> GetEntities(int x, int y, int radius);

    bool CanWalkFromTo(int x, int y, int x2, int y2);

    void DisplayMessage(string message); // This isn't the way to do it.
}

public partial class Model : ModelAPI
{
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
            if (e.position.x == x && e.position.y == y)
            {
                return e;
            }
        }
        return null;
    }

    public List<Entity> GetEntities(int x, int y, int radius)
    {
        List<Entity> inRadius = new List<Entity>();
        foreach (Entity e in entities)
        {
            if (Math.Abs(e.position.x - x) <= radius && Math.Abs(e.position.y - y) <= radius)
            {
                inRadius.Add(e);
            }
        }
        return inRadius;
    }

    public bool CanWalkFromTo(int x, int y, int x2, int y2)
    {
        return map.GetCell(x2, y2) != -1;
    }

    public void DisplayMessage(string message)
    {
        GD.Print(message);
    }
}
