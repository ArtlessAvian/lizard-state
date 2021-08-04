using Godot;
using System;
using System.Collections.Generic;

public class GetAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        if (model.GetMap().GetCell(e.position.x, e.position.y) == 2)
        {
            model.CoolerApiEvent(-1, "Print", "Got the moss.");
            e.nextMove += 100;
            return true;
        }

        return false;
    }

    public override bool IsValid(Model model, Entity e)
    {
        if (model.GetMap().GetCell(e.position.x, e.position.y) == 2)
        {
            return true;
        }

        return false;
    }
}
