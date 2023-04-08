using Godot;
using Godot.Collections;
using System.Collections.Generic;

// Hits a short cone instantly, but moves you forward.
// Should be unsafe on "block," and knockdown on hit.
public class SlashAbility : Action
{
    public SlashAbility()
    {

    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);

        model.CoolerApiEvent(new Dictionary(){
            {"subject", e.id},
            {"action", "AttackActive"},
            {"args", new Vector2(targetPos.x, targetPos.y)},
            {"flavorTags", new Array{"Bump"}}
        });

        // TODO: Pain point, get entities in cone.
        foreach (Entity targeted in model.GetEntitiesInCone(e.position, 2, targetPos - e.position, 45))
        {
            if (targeted == e) { continue; }

            targeted.health -= 2;
            targeted.queuedAction = null;

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Hit"},
                {"object", targeted.id},
                {"damage", 3},
                {"flavorTags", new Array{"Bump"}}
            });
        }
        // TODO: Sort before knockback.
        foreach (Entity targeted in model.GetEntitiesInCone(e.position, 2, targetPos - e.position, 45))
        {
            if (targeted == e) { continue; }
            ActionUtils.ApplyKnockback(model, e, targeted, 1);
        }

        AbsolutePosition step = GridHelper.StepTowards(e.position, targetPos, 1);
        if (model.GetEntityAt(step) is null)
        {
            e.position = step;
            model.CoolerApiEvent(e.id, "Move", new Vector2(step.x, step.y));
        }

        e.nextMove += 1;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);
        // TODO: Add raycast to target.

        if (GridHelper.Distance(e.position, targetPos) > 2)
        {
            return false;
        }

        return true;
    }

    public override (int, int) Range => (1, 2);
    public override TargetingType.Type TargetingType => new TargetingType.Cone { radius = 2, sectorDegrees = 45 };
}
