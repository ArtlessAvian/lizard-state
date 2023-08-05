using Godot;
using System.Collections.Generic;
using LizardState.Engine;

public class DashPunchAbility : CrawlAction
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e)) { return false; }

        AbsolutePosition targetPos = GetTargetPos(e.position);

        if (targetPos.x == e.position.x && targetPos.y == e.position.y)
        {
            return true;
        }

        AbsolutePosition place = e.position;
        Entity target = null;
        foreach (AbsolutePosition tile in GridHelper.RayThrough(e.position, targetPos))
        {
            if (tile != e.position)
            {
                if (GridHelper.Distance(tile, e.position) > 5) { break; }
                if ((target = model.GetEntityAt(tile)) != null) { break; }
                if (!model.CanWalkFromTo(place, tile)) { break; }
            }
            place = tile;
        }

        e.position = place;
        model.CoolerApiEvent(e.id, "Dash", new Vector2(e.position.x, e.position.y));

        if (target != null)
        {
            ReachAttackAction thing = GD.Load<ReachAttackAction>("res://Crawler/Model/Attacks/ReachAttacks/HellaKnockback.tres");
            CrawlAction subaction = GD.Load<CSharpScript>("res://Crawler/Model/Actions/ReachAttackFollowup.cs").New(thing) as CrawlAction;
            // Action subaction = e.species.rushAttack.Duplicate() as Action;
            subaction.SetTarget(target.position);
            subaction.Do(model, e);
        }

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }

    public override (int, int) Range => (1, 5);
    public override TargetingType.Type TargetingType => new TargetingType.Line { };
}
