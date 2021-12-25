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

        RushAttackData data = e.species.rushAttack ?? ResourceLoader.Load<RushAttackData>("res://Crawler/Model/Attacks/RushAttacks/ExpectedOne.tres");
 
        GD.Print(data.MissChance);

        (int x, int y) targetPos = GetTargetPos(e.position);
        Entity targeted = model.GetEntityAt(targetPos);

        model.CoolerApiEvent(-1, "SmallWait");

        model.CoolerApiEvent(e.id, "StartAttack", new Vector2(targetPos.x, targetPos.y));
        
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
            targeted.health -= data.damagePerHit;

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Rush"},
                {"object", targeted.id},
                {"damage", data.damagePerHit}
            });
            // model.Debug($"{targeted.ResourceName}: ouch");
        }
        
        if (targeted.health <= 0)
        {
            targeted.downed = true;
            targeted.nextMove = -1;
            model.CoolerApiEvent(targeted.id, "Downed");
        }

        model.CoolerApiEvent(-1, "SmallWait");
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
