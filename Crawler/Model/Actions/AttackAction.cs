using Godot;
using System.Collections.Generic;

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
        // data ??= GD.Load<AttackData>("res://Crawler/Model/Attacks/Instances/BasicAttack.tres");
        if (data is null)
        {
            this.data = GD.Load<AttackData>("res://Crawler/Model/Attacks/Instances/BasicAttack.tres");
        }
    }

    public override bool Do(ModelAPI api, Entity e)
    {
        if (e.energy < data.energy)
        {
            return false;
        }

        // TODO: Replace with raycast.
        (int x, int y) targetPos = GetTargetPos(e.position);

        Entity targeted = api.GetEntityAt(targetPos);
        if ((targeted is null) || targeted == e)
        {
            return false;
        }

        if (GridHelper.Distance(e.position, targeted.position) > data.range)
        {
            return false;
        }

        e.energy -= data.energy;

        api.ApiEvent(new ModelEvent(-1, "Wait"));
        
        api.ApiEvent(new ModelEvent(e.id, "StartAttack", targetPos));
        
        AttackResult result = data.TryAttack(targeted, e.nextMove); // e.nextMove is now!
        targeted.GetAttacked(api, result, e.id);

        // TODO: e is possibly null. Investigate?
        e.nextMove += data.recovery;

        api.ApiEvent(new ModelEvent(-1, "Wait"));
        return true;
    }

    public override (int, int) Range => (1, data.range);
}
