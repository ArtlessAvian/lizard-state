using Godot;
using Godot.Collections;
using System.Collections.Generic;
using LizardState.Engine;

public class StepSweepAbility : CrawlAction
{
    public StepSweepAbility()
    {

    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);

        AbsolutePosition step = GridHelper.StepTowards(e.position, targetPos, 1);

        if (model.GetEntityAt(step) is null)
        {
            e.position = step;
            model.CoolerApiEvent(e.id, "Move", new Vector2(step.x, step.y));
        }

        model.CoolerApiEvent(new Dictionary(){
            {"subject", e.id},
            {"action", "AttackActive"},
            {"args", new Vector2(targetPos.x, targetPos.y)},
            {"flavorTags", new Array{"Sweep"}}
        });

        // TODO: Pain point, get entities in cone.
        foreach (Entity targeted in model.GetEntitiesInLOS(e.position, 1))
        {
            if (targeted == e) { continue; }

            if (TimestampBefore((model.time, e.id), (targeted.blockingUntil, targeted.id)))
            {
                model.CoolerApiEvent(new Dictionary(){
                    {"subject", targeted.id},
                    {"action", "Block"},
                    {"object", e.id},
                    {"flavorTags", new Array{"Sweep"}},
                });
                continue;
            }

            targeted.health -= 2;
            targeted.queuedAction = null;

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Hit"},
                {"object", targeted.id},
                {"damage", 3},
                {"swept", true},
                {"flavorTags", new Array{"Sweep"}}
            });
            targeted.KnockdownForTurns(1, model.time, e.id);
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

    // TODO: Genericize.
    // Elementwise compare, like Python's default tuple compare.
    // (Yes, there's a nice one line expression. It's messy though.)
    private static bool TimestampBefore((int turn, int turntaker) a, (int turn, int turntaker) b)
    {
        if (a.turn < b.turn) { return true; }
        if (a.turn == b.turn)
        {
            if (a.turntaker < b.turntaker) { return true; }
        }
        return false;
    }

    public override (int, int) Range => (1, 2);
    public override TargetingType.Type TargetingType => new TargetingType.Cone(45);
}
