using Godot;
using System.Collections.Generic;
using Godot.Collections;
using LizardState.Engine;

// Neutral skip tool. Like a FF Dragoon jump.
public class JumpAbility : CrawlAction
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);
        e.energy -= 1;

        model.CoolerApiEvent(e.id, "Jump", new Vector2(targetPos.x, targetPos.y));
        e.state = Entity.EntityState.INTANGIBLE;

        model.CoolerApiEvent(e.id, "Move", new Vector2(targetPos.x, targetPos.y));
        e.position = targetPos;

        CSharpScript followup = GD.Load("res://BaseGame/Actions/Abilities/JumpFollowup.cs") as CSharpScript;
        e.queuedAction = followup.New() as CrawlAction;
        e.queuedAction = e.queuedAction.SetTarget(targetPos);

        e.nextMove += 1;

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

    public override (int min, int max) Range => (1, 6);
    public override TargetingType.Type TargetingType => new TargetingType.Smite { radius = 1 };
}
