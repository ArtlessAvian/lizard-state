using Godot;
using System;
using System.Collections.Generic;

public class ExitAction : Action
{
    public override bool Do(ModelAPI api, Entity e)
    {
        if (!IsValid(api, e))
        {
            return false;
        }

        (int x, int y) = GetTargetPos(e.position);

        if (api.GetMap().GetCell(x, y) == 5)
        {
            api.CoolerApiEvent(-1, "Print", "You leave the cave.");
            api.CoolerApiEvent(0, "Exit");
            e.nextMove = -1;
            return true;
        }
        return false;
    }

    public override bool IsValid(ModelAPI api, Entity e)
    {
        (int x, int y) = GetTargetPos(e.position);
        if (api.GetMap().GetCell(x, y) == 5)
        {
            return true;
        }
        return false;
    }

    public override (float, float) Range => (1, 1.5f);
}
