using Godot;
using System.Collections.Generic;

public class AttackAction : Action
{
    (int x, int y) direction;
    AttackData data;

    public AttackAction((int x, int y) direction, AttackData data = null)
    {
        this.direction = direction;
        this.data = data;
    }

    public bool IsAimed()
    {
        return true;
    }

    public bool Do(ModelAPI api, Entity e)
    {
        this.data = data ?? GD.Load<AttackData>("res://Crawler/Model/Attacks/BasicAttack.tres");

        // TODO: Replace with raycast.
        Entity target = api.GetEntityAt(e.position.x + direction.x, e.position.y + direction.y);
        if ((target is null) || target == e)
        {
            return false;
        }

        api.NewEvent(new ModelEvent(-1, "Wait"));
        
        api.NewEvent(new ModelEvent(e.id, "StartAttack", direction));
        
        AttackResult result = data.TryAttack(target, e.nextMove); // e.nextMove is now!
        target.GetAttacked(api, result, e.id);

        // TODO: e is possibly null. Investigate?
        e.nextMove += 10;

        api.NewEvent(new ModelEvent(-1, "Wait"));
        return true;
    }
}
