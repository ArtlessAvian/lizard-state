using Godot;
using Godot.Collections;
using System.Collections.Generic;

// "Swaps" you and a target. May rescue a partner, or bring an enemy into range.
public class RepositionAbility : Action
{
    public RepositionAbility()
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
        Entity rescuee = model.GetEntityAt(targetPos);

        e.position = targetPos;
        model.CoolerApiEvent(e.id, "Dash", new Vector2(e.position.x, e.position.y));

        if (rescuee is object)
        {
            AbsolutePosition midpoint = originalPos;
            foreach (AbsolutePosition tile in GridHelper.LineBetween(targetPos, originalPos))
            {
                if (GridHelper.Distance(targetPos, tile) >= 2)
                {
                    midpoint = tile;
                    break;
                }
            }

            rescuee.position = midpoint;
            model.CoolerApiEvent(rescuee.id, "Dash", new Vector2(midpoint.x, midpoint.y));

            rescuee.StunForTurns(1, model.time, e.id);
            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Hit"},
                {"object", rescuee.id},
                {"damage", 0},
            });
        }
        else
        {
            // Some penalty.
            // e.KnockdownForTurns(1, model.time, e.id);
            // model.CoolerApiEvent(new Dictionary(){
            //     {"subject", e.id},
            //     {"action", "Hit"},
            //     {"object", e.id},
            //     {"damage", 0},
            //     {"swept", true},
            // });
        }

        e.nextMove += 1;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);

        if (GridHelper.Distance(e.position, targetPos) > Range.max)
        {
            return false;
        }

        return true;
    }

    public override (int min, int max) Range => (1, 5);
    public override TargetingType.Type TargetingType => new TargetingType.Line { range = 5 };
}
