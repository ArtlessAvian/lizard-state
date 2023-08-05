using Godot;
using Godot.Collections;
using System.Collections.Generic;

// Charge move. Comes out instantly.
public class SpitPoisonAbility : CrawlAction
{
    public SpitPoisonAbility()
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
        foreach (Entity targeted in model.GetEntitiesInCone(e.position, 3, targetPos - e.position, 45))
        {
            if (targeted == e) { continue; }

            targeted.health -= 2;
            targeted.queuedAction = null;

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Hit"},
                {"object", targeted.id},
                {"damage", 2},
                {"flavorTags", new Array{"Bump"}}
            });

            targeted.StunForTurns(1, model.time, e.id);
        }

        e.nextMove += 1;
        e.chargeStart = model.time;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        if (model.time - e.chargeStart < 3)
        {
            return false;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);
        // TODO: Add raycast to target.

        return true;
    }

    public override (int, int) Range => (1, 3);
    public override TargetingType.Type TargetingType => new TargetingType.Cone(45);
}
