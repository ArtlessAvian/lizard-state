using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class ExplodeAbility : Action
{
    AttackData data;
    public ExplodeAbility()
    {
        this.data = GD.Load<AttackData>("res://Crawler/Model/Attacks/Instances/BasicAttack.tres");
    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        model.CoolerApiEvent(-1, "Wait");

        model.CoolerApiEvent(e.id, "StartAttack");

        // for each target
        foreach (Entity targeted in model.GetEntitiesInLOS(e.position, 20))
        {
            AttackResult result = data.DoAttack(targeted, e.nextMove); // e.nextMove is now!
            result.damage = 29;
            targeted.TakeDamage(result);

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
        }
        e.nextMove += data.recovery;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }

    // public override (float min, float max) Range => (0, 0);
}
