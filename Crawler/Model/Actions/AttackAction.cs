using Godot;
using Godot.Collections;

public class AttackAction : Action
{
    AttackData data;

    public AttackAction(Entity e, int id = -1)
    {
        if (id < 0)
        {
            this.data = e.species.bumpAttack;
        }
        else
        {
            this.data = e.species.attacks[id];
        }

        if (data is null)
        {
            this.data = GD.Load<AttackData>("res://Crawler/Model/Attacks/Instances/BasicAttack.tres");
        }
    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        (int x, int y) targetPos = GetTargetPos(e.position);
        Entity targeted = model.GetEntityAt(targetPos);

        e.energy -= data.energy;

        model.CoolerApiEvent(-1, "Wait");

        model.CoolerApiEvent(e.id, "StartAttack");
        // for each target
        
        AttackResult result = data.TryAttack(targeted, e.nextMove); // e.nextMove is now!
        targeted.GetAttacked(result);

        Dictionary hitResult = result.ToDict();
        hitResult.Add("target", targeted.id);
        
        // add to array
        // end for

        Dictionary attackResult = new Dictionary(){
            {"subject", e.id},
            {"action", "Hit"},
            {"object", targeted.id},
            // {"targetPos", new Vector2(targetPos.x, targetPos.y)},
            {"hit", hitResult}
        };
        model.CoolerApiEvent(attackResult);

        // for each target
        if (targeted.health <= 0)
        {
            model.CoolerApiEvent(targeted.id, "Downed");
        }

        e.nextMove += data.recovery;

        model.CoolerApiEvent(-1, "Wait");
        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
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

    public override (float, float) Range => (1, data.range);
}
