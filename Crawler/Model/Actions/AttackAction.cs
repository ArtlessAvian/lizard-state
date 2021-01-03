using Godot;
using System.Collections.Generic;

public class AttackAction : Action
{
    (int x, int y) direction;

    public AttackAction(object args) 
    {
        direction = ((int, int))args;
    }

    public bool Do(ModelAPI api, List<ModelEvent> eventQueue, Entity e)
    {
        Entity target = api.GetEntityAt(e.position.x + direction.x, e.position.y + direction.y);
        eventQueue.Add(new ModelEvent(e, "Face", direction));

        if ((target is null) || target == e)
        {
            return false;
        }

        // All logic here
        bool crit = GD.Randf() < 0.1;
        int damage = crit ? 3 : 1;

        e.nextMove += 10;
        target.health -= damage;
        if (crit)
        {
            target.nextMove = e.nextMove;
        }
        if (target.health <= 0)
        {
            target.downed = true;
            target.nextMove = -1;
        }

        eventQueue.Add(new ModelEvent(null, "Wait"));
        eventQueue.Add(new ModelEvent(null, "Print", $"{e.species.displayName} hits {target.species.displayName}!"));
        if (crit)
            eventQueue.Add(new ModelEvent(null, "Print", $"{target.species.displayName} stumbles!!"));
        if (target.downed)
            eventQueue.Add(new ModelEvent(null, "Print", $"{target.species.displayName} is downed!!"));

        eventQueue.Add(new ModelEvent(e, "Attack", damage, target));
        
        eventQueue.Add(new ModelEvent(null, "Wait"));

        return true;
    }
}
