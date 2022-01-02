using Godot;
using Godot.Collections;

public class RushAttackAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        // TODO: This NPEs sometimes
        RushAttackData data = e.species.rushAttack ?? ResourceLoader.Load<RushAttackData>("res://Crawler/Model/Attacks/RushAttacks/ExpectedTwo.tres");

        (int x, int y) targetPos = GetTargetPos(e.position);
        Entity targeted = model.GetEntityAt(targetPos);
        
        if (GD.Randf() < data.MissChance)
        {
            e.nextMove += data.recovery;

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Miss"},
                {"object", targeted.id}
            });
        }
        else
        {
            // TODO: Move into entity?
            
            // model.Debug($"{targeted.ResourceName}: ouch");
            targeted.health -= data.damagePerHit;
            targeted.queuedAction = null;

            if (targeted.health <= 0)
            {
                targeted.downed = true;
                targeted.nextMove = -1;
            }
            
            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Rush"},
                {"object", targeted.id},
                {"damage", data.damagePerHit}
            });

            if (targeted.health <= 0)
            {
                model.CoolerApiEvent(targeted.id, "Downed");
            }
        }

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);
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
}
