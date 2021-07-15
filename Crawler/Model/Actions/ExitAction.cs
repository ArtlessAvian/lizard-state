using Godot;
using System;
using System.Collections.Generic;

public class ExitAction : ActionTargeted
{
    public override bool Do(ModelAPI api, Entity e)
    {
        (int x, int y) = GetTargetPos(e.position);

        if (api.GetMap().GetCell(x, y) == 5)
        {
            api.ApiEvent(new ModelEvent(-1, "Print", "You leave the cave. (You win!)"));
            e.nextMove = -1;
            return true;
        }

        return false;
    }
}
