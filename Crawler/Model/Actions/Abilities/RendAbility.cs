using Godot;
using Godot.Collections;
using System.Collections.Generic;
using LizardState.Engine;

// Multihit. Pretty generic. Edgy name I don't like.
public class RendAbility : CrawlAction
{
    [Export]
    int damagePerHit = 1;

    public RendAbility()
    {

    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);
        Entity targeted = model.GetEntityAt(targetPos);

        for (int i = 0; i < 4; i++)
        {
            targeted.health -= damagePerHit;
            targeted.queuedAction = null;

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "AttackActive"},
                {"args", new Vector2(targetPos.x, targetPos.y)},
                {"flavorTags", new Array{"Bump"}}
            });

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Hit"},
                {"object", targeted.id},
                {"damage", damagePerHit},
                {"flavorTags", new Array{"Bump"}}
            });
        }

        e.nextMove += 1;
        // e.energy -= 2;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);
        // TODO: Add raycast to target.

        if (GridHelper.Distance(e.position, targetPos) > 1.5f)
        {
            return false;
        }

        Entity targeted = model.GetEntityAt(targetPos);
        if ((targeted is null) || targeted == e)
        {
            return false;
        }

        return true;
    }

    public override (int, int) Range => (1, 1);
}
