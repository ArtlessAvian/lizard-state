using Godot;
using System.Collections.Generic;

public class AttackAction : Action
{
    public struct AttackResult
    {
        public int damage;
        public bool crit;

        public AttackResult(int _)
        {
            crit = GD.Randf() < 0.1;
            damage = crit ? 3 : 1;
        }
    }

    (int x, int y) direction;

    public AttackAction(object args) 
    {
        direction = ((int, int))args;
    }

    public bool Do(ModelAPI api, List<ModelEvent> eventQueue, Entity e)
    {
        eventQueue.Add(new ModelEvent(e, "Face", direction));
        Entity target = api.GetEntityAt(e.position.x + direction.x, e.position.y + direction.y);

        if ((target is null) || target == e)
        {
            return false;
        }

        // All logic here
        AttackResult roll = new AttackResult(3);

        e.nextMove += 10;
        target.TakeDamage(roll.damage, roll.crit ? e.nextMove + 5 : 0);

        eventQueue.Add(new ModelEvent(null, "Wait"));
        
        // Consider moving to actors?
        eventQueue.Add(new ModelEvent(null, "Print", $"{e.species.displayName} hits {target.species.displayName}!"));
        if (roll.crit)
            eventQueue.Add(new ModelEvent(null, "Print", $"{target.species.displayName} stumbles!!"));
        if (target.downed)
            eventQueue.Add(new ModelEvent(null, "Print", $"{target.species.displayName} is downed!!"));

        eventQueue.Add(new ModelEvent(e, "Attack", roll, target));
        
        eventQueue.Add(new ModelEvent(null, "Wait"));

        return true;
    }
}
