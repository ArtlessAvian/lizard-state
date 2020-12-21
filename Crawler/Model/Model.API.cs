using Godot;
using System;

public interface ModelAPI
{
    Entity GetEntity(int id);
    Entity GetPlayer();
    bool CanWalkFromTo(int x, int y, int x2, int y2);
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

    public bool CanWalkFromTo(int x, int y, int x2, int y2)
    {
        return map.GetCell(x2, y2) != -1;
    }
}
