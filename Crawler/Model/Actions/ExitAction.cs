using Godot;
using System;
using System.Collections.Generic;

public class ExitAction : Action
{
    public bool Do(ModelAPI api, Entity e)
    {
        if (api.GetMap().GetCell(e.position.x, e.position.y) != 4)
        {
            return false;
        }

        e.nextMove += 10;
        return true;
    }
}
