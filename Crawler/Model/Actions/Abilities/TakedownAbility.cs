using Godot;
using Godot.Collections;
using System.Collections.Generic;

// Invincible move, disables the enemy.
// Kind of silly. Not sure what I think.
public class TakedownAbility : Action
{
    public TakedownAbility()
    {

    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        (int x, int y) targetPos = GetTargetPos(e.position);
        Entity target = model.GetEntityAt(targetPos);

        model.CoolerApiEvent(e.id, "Dash", new Vector2(targetPos.x, targetPos.y));

        e.position = targetPos;
        e.KnockdownForTurns(1, model.time, e.id);
        model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Hit"},
                {"object", e.id},
                {"damage", 0},
                {"swept", true}
            });

        if (target != null)
        {
            target.KnockdownForTurns(1, model.time, e.id);
            model.CoolerApiEvent(new Dictionary(){
                    {"subject", e.id},
                    {"action", "Hit"},
                    {"object", target.id},
                    {"damage", 0},
                    {"swept", true}
                });
        }

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);
        // TODO: Add raycast to target.

        if (GridHelper.Distance(e.position, targetPos) > 5)
        {
            return false;
        }

        return true;
    }

    public override (int, int) Range => (1, 5);
    public override TargetingType.Type TargetingType => new TargetingType.Ray(false, true);
}
