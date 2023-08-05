using Godot;
using Godot.Collections;
using System.Collections.Generic;
using LizardState.Engine;

public class CrossupAbility : CrawlAction
{
    public CrossupAbility()
    {

    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);

        if (model.GetEntityAt(targetPos) is Entity targeted)
        {
            targeted.position = GridHelper.StepTowards(targeted.position, e.position, 1);
            e.position = targetPos;

            model.CoolerApiEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));
            model.CoolerApiEvent(targeted.id, "Move", new Vector2(targeted.position.x, targeted.position.y));

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "AttackActive"},
                {"args", new Vector2(targeted.position.x, targeted.position.y)},
            });

            ActionUtils.ApplyDamage(model, e, targeted, 1);
            // TODO: Apply the knockback along the original line between e.position and targeted.position.
            ActionUtils.ApplyKnockback(model, e, targeted, 1);

            targeted.KnockdownForTurns(1, model.time, e.id);
            model.CoolerApiEvent(new Dictionary(){
                {"subject", targeted.id},
                {"action", "Knockdown"},
            });
        }
        else
        {
            // just go there.
            e.position = targetPos;
        }

        e.nextMove += 1;

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

    public override (int min, int max) Range => (1, 2);
    public override TargetingType.Type TargetingType => new TargetingType.Line { };
}
