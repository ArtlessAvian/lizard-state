using Godot;
using System;
using System.Collections.Generic;

public class ExitAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);

        if (model.GetMap().GetCell(targetPos.x, targetPos.y) == 5)
        {
            model.CoolerApiEvent(-1, "Print", "You leave the cave.");
            model.CoolerApiEvent(0, "Exit");
            e.nextMove = -1;
            model.done = true;
            return true;
        }
        return false;
    }

    public override bool IsValid(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);
        if (model.GetMap().GetCell(targetPos.x, targetPos.y) == 5)
        {
            return true;
        }
        return false;
    }

    public override IEnumerable<string> GetWarnings(Model model, Entity e)
    {
        if (!e.hasEaten)
        {
            yield return "You haven't eaten yet. Really leave now?";
        }
    }
}
