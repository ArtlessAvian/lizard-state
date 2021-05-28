using Godot;
using System;
using System.Collections.Generic;

public class ExitAction : Action
{
    public bool Do(ModelAPI api, Entity e)
    {
        // GD.Print(api.GetMap().map.GetCell(e.position.x, e.position.y));
        e.nextMove += 10;
        return true;
    }
}
