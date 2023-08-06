using Godot;
using System;
using System.Collections.Generic;
using LizardState.Engine;

public class MoveOrAttackAction : CrawlAction
{
    public override bool Do(Model model, Entity e)
    {
        return GetMacroedAction(model, e).Do(model, e);
    }

    public override bool IsValid(Model model, Entity e)
    {
        return GetMacroedAction(model, e).IsValid(model, e);
        // return GridHelper.Distance(e.position, GetTargetPos(e.position)) <= 1.5f;
        // return true; // TODO: This one is tough. I could just make every action object and || them. (See MoveAction.cs too).
    }

    public override IEnumerable<string> GetWarnings(Model model, Entity e)
    {
        return GetMacroedAction(model, e).GetWarnings(model, e);
    }

    // only makes sense for abilities that might be aimed.
    // public override (float, float) Range => (1, 10);

    public CrawlAction GetMacroedAction(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);

        if (model.GetMap().tiles.GetCell(targetPos.x, targetPos.y) == 5)
        {
            return new ExitAction().SetTarget(targetPos);
        }

        Entity targeted = model.GetEntityAt(targetPos);
        if (!(targeted is null) && targeted.team != e.team)
        {
            return e.species.bumpAttack.SetTarget(targetPos);
        }
        return new MoveAction().SetTarget(targetPos);
    }
}