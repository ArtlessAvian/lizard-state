using Godot;
using System;
using System.Collections.Generic;

public class GetAction : Action
{
    public override bool Do(ModelAPI api, Entity e)
    {
        if (api.GetMap().GetCell(e.position.x, e.position.y) == 2)
        {
            api.ApiEvent(new ModelEvent(-1, "Print", "Got the moss."));
            e.nextMove += 100;
            return true;
        }

        return false;
    }
}
