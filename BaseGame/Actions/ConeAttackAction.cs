using Godot;
using Godot.Collections;
using System.Collections.Generic;
using LizardState.Engine;

// Charge move. Comes out instantly.
public class ConeAttackAction : CrawlAction
{
    public ConeAttackAction()
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
        foreach (Entity targeted in model.GetEntitiesInCone(e.position, 3, targetPos - e.position, 20))
        {
            if (targeted == e) { continue; }

            targeted.health -= 2;
            targeted.queuedAction = null;

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Hit"},
                {"object", targeted.id},
                {"damage", 2},
                {"flavorTags", new Array{"Psychic"}}
            });
        }

        e.nextMove += 2;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        if (model.Distance(e.position, GetTargetPos(e.position)) > 3)
        {
            return false;
        }
        return true;
    }

    public override (int, int) Range => (1, 3);
    public override TargetingType.Type TargetingType => new TargetingType.Cone(20);
}
