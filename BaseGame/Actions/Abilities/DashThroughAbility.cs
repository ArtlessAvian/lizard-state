using Godot;
using System.Collections.Generic;
using Godot.Collections;
using LizardState.Engine;

// Threatens long range, with startup.
// Think Chipp Alpha Blade or something.
public class DashThroughAbility : CrawlAction
{
    public DashThroughAbility()
    {

    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);

        e.nextMove += 1;
        model.CoolerApiEvent(e.id, "AttackStartup", new Vector2(targetPos.x, targetPos.y));

        CSharpScript followup = GD.Load("res://BaseGame/Actions/Abilities/DashThroughFollowup.cs") as CSharpScript;
        e.queuedAction = followup.New() as CrawlAction;
        e.queuedAction = e.queuedAction.SetTarget(targetPos);

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        if (e.energy < 1) { return false; }

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
