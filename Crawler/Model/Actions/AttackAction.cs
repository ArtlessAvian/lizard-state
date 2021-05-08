using Godot;
using System.Collections.Generic;

public class AttackAction : ActionTargeted
{
    AttackData data;

    public AttackAction(AttackData data = null)
    {
        this.data = data;
    }

    public override bool Do(ModelAPI api, Entity e)
    {
        this.data = data ?? GD.Load<AttackData>("res://Crawler/Model/Attacks/BasicAttack.tres");

        // TODO: Replace with raycast.
        Entity targeted = api.GetEntityAt(target);
        if ((targeted is null) || targeted == e)
        {
            return false;
        }

        if (api.Distance(e.position, targeted.position) > data.range)
        {
            return false;
        }

        api.NewEvent(new ModelEvent(-1, "Wait"));
        
        api.NewEvent(new ModelEvent(e.id, "StartAttack", target));
        
        AttackResult result = data.TryAttack(targeted, e.nextMove); // e.nextMove is now!
        targeted.GetAttacked(api, result, e.id);

        // TODO: e is possibly null. Investigate?
        e.nextMove += 10;

        api.NewEvent(new ModelEvent(-1, "Wait"));
        return true;
    }
}
