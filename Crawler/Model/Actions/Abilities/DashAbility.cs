using Godot;
using System.Collections.Generic;
using LizardState.Engine;

public class DashAbility : CrawlAction
{
    public DashAbility()
    {

    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);

        e.position = targetPos;
        e.nextMove += 2;

        model.CoolerApiEvent(e.id, "Dash", new Vector2(e.position.x, e.position.y));

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);

        if (GridHelper.Distance(e.position, targetPos) > Range.max)
        {
            return false;
        }

        return true;
    }

    public override (int min, int max) Range => (1, 5);
    public override TargetingType.Type TargetingType => new TargetingType.Line { };
}
