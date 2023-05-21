using Godot;
using System.Collections.Generic;
using Godot.Collections;

// Comes after DashThroughAbility. Godot needs one class per file :P
public class DashThroughFollowup : Action
{
    public DashThroughFollowup()
    {

    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AbsolutePosition originalPos = e.position;
        AbsolutePosition targetPos = GetTargetPos(e.position);
        e.position = targetPos;

        model.CoolerApiEvent(e.id, "Dash", new Vector2(e.position.x, e.position.y));

        model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "AttackActive"},
                {"args", new Vector2(targetPos.x, targetPos.y)},
            });

        bool hitAnyone = false;
        foreach (AbsolutePosition tile in GridHelper.LineBetween(originalPos, targetPos))
        {
            GD.Print(tile.x, tile.y);
            Entity hit = model.GetEntityAt(tile);
            if (hit is object && hit != e && hit.team != e.team)
            {
                hitAnyone = true;
                hit.health -= 1;
                // hit.StunForTurns(0, model.time, e.id);

                model.CoolerApiEvent(new Dictionary(){
                    {"subject", e.id},
                    {"action", "Hit"},
                    {"object", hit.id},
                    {"damage", 1}
                });
            }
        }

        e.nextMove += hitAnyone ? 0 : 1;
        e.energy -= 1;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }

    public override (int min, int max) Range => (1, 5);
    public override TargetingType.Type TargetingType => new TargetingType.Ray(true, true);
}
