using Godot;
using System;

public interface ModelAPI
{
    Entity GetEntity(int id);
    Entity GetPlayer();
    Entity GetEntityAt(int x, int y);
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

    public bool CanWalkFromTo(int x, int y, int x2, int y2)
    {
        return map.GetCell(x2, y2) != -1;
    }

    public void DisplayMessage(string message)
    {
        GD.Print(message);
    }
}
