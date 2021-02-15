using Godot;
using System.Collections.Generic;

public class AttackAction : Action
{
    (int x, int y) direction;
    AttackData data;

    public AttackAction((int x, int y) direction, AttackData data = null)
    {
        this.direction = direction;
        this.data = data ?? GD.Load<AttackData>("res://Crawler/Model/Attacks/BasicAttack.tres");
    }

    public bool Do(ModelAPI api, Entity e)
    {
        // TODO: Replace with raycast.
        Entity target = api.GetEntityAt(e.position.x + direction.x, e.position.y + direction.y);
        if ((target is null) || target == e)
        {
            return false;
        }

        api.NewEvent(new ModelEvent(-1, "Wait"));

        int timeNow = e.nextMove;
        
        // TODO: Figure out NPE. e was null!
        e.nextMove += 10;
        AttackResult result = data.TryAttack(target, timeNow);

        api.NewEvent(new ModelEvent(e.id, "Attack", null, target.id));
        target.GetAttacked(api, result);

        api.NewEvent(new ModelEvent(-1, "Wait"));
        return true;
    }
}
