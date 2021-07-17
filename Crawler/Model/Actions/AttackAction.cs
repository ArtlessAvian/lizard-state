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
        if (this.data is null)
        {
            this.data = GD.Load<AttackData>("res://Crawler/Model/Attacks/BasicAttack.tres");
            GD.PrintErr($"{e.species.ResourcePath} missing attacks?");
        }

        if (e.energy < this.data.energy)
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

        e.energy -= this.data.energy;

        api.ApiEvent(new ModelEvent(-1, "Wait"));
        
        api.ApiEvent(new ModelEvent(e.id, "StartAttack", targetPos));
        
        AttackResult result = data.TryAttack(targeted, e.nextMove); // e.nextMove is now!
        targeted.GetAttacked(api, result, e.id);

        // TODO: e is possibly null. Investigate?
        e.nextMove += data.recovery;

        api.ApiEvent(new ModelEvent(-1, "Wait"));
        return true;
    }
}
