using Godot;
using Godot.Collections;
using System.Collections.Generic;

// Cancels someones action. Sets their turn to run *immediately.*
public class StartleAbility : CrawlAction
{
    public StartleAbility()
    {

    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        AbsolutePosition targetPos = GetTargetPos(e.position);

        if (model.GetEntityAt(targetPos) is Entity targeted)
        {
            // technically this "goes back in time" which is not obvious.
            targeted.nextMove = model.time - (e.id <= targeted.id ? 1 : 0);

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Hit"},
                {"object", targeted.id},
                {"damage", 0},
            });
        }

        e.nextMove += 1;
        e.chargeStart = model.time;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);

        if (GridHelper.Distance(e.position, targetPos) > 5)
        {
            return false;
        }

        return true;
    }

    public override (int, int) Range => (1, 5);
    public override TargetingType.Type TargetingType => new TargetingType.Smite(0);
}
