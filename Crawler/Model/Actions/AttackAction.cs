using Godot;
using Godot.Collections;

public class AttackAction : Action
{
    public AttackAction()
    {
    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AttackData data = e.species.bumpAttack;
 
        (int x, int y) targetPos = GetTargetPos(e.position);
        Entity targeted = model.GetEntityAt(targetPos);

        // e.energy -= data.energy;

        model.CoolerApiEvent(-1, "Wait");

        model.CoolerApiEvent(e.id, "StartAttack", new Vector2(targetPos.x, targetPos.y));
        AttackResult hitResult = data.DoAttack(targeted, e.nextMove); // e.nextMove is now!
        targeted.TakeDamage(hitResult);

        Dictionary attackResult = new Dictionary(){
            {"subject", e.id},
            {"action", "Hit"},
            {"object", targeted.id},
            // {"targetPos", new Vector2(targetPos.x, targetPos.y)},
            {"hit", hitResult.ToDict()},
            {"combo", targeted.comboCounter}
        };

        model.CoolerApiEvent(attackResult);
        
        if (targeted.health <= 0)
        {
            model.CoolerApiEvent(targeted.id, "Downed");
        }

        if (!hitResult.stuns)
        {
            e.nextMove += data.recovery;
        }

        model.CoolerApiEvent(-1, "Wait");
        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        AttackData data = e.species.bumpAttack;

        if (e.species.bumpAttack != data && !e.species.attacks.Contains(data))
        {
            return false;
        }

        if (e.energy < data.energy)
        {
            return false;
        }

        (int x, int y) targetPos = GetTargetPos(e.position);
        // TODO: Add raycast to target.

        if (GridHelper.Distance(e.position, targetPos) > data.range)
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

    public override (float, float) Range => (1, 1.5f);
}
