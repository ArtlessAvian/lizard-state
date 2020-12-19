using Godot;
using System;

public interface ModelAPI
{
    bool CanWalkFromTo(int x, int y, int x2, int y2);
}

public partial class Model : ModelAPI
{
    public bool CanWalkFromTo(int x, int y, int x2, int y2)
    {
        return map.GetCell(x2, y2) != -1;
    }
}
