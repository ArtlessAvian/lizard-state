using Godot;
using System.Collections.Generic;

public class SpinAbility : Action
{
    static Attack attack = new Attack(0.2f, 0.8f, 5, 0, 5);

    public SpinAbility() 
    {

    }

    public bool Do(ModelAPI api, List<ModelEvent> eventQueue, Entity e)
    {
        e.nextMove += 10;

        List<Entity> targets = api.GetEntitiesInRadius(e.position.x, e.position.y, 1);
        
        eventQueue.Add(new ModelEvent(null, "Wait"));
        eventQueue.Add(new ModelEvent(null, "Print", $"{e.species.displayName} spins!"));

        foreach (Entity target in targets)
        {
            if (target.team == e.team) {continue;}

            AttackResult roll = new AttackResult(target, attack, e);
            target.TakeDamage(roll);

            eventQueue.Add(new ModelEvent(e, "Attack", roll, target));

            if (roll.hit)
                eventQueue.Add(new ModelEvent(null, "Print", $"Hit the {target.species.displayName}!"));
            else
                eventQueue.Add(new ModelEvent(null, "Print", $"Missed the {target.species.displayName}."));

            if (target.downed)
                eventQueue.Add(new ModelEvent(null, "Print", $"{target.species.displayName} is downed!!"));
        }

        eventQueue.Add(new ModelEvent(null, "Wait"));

        return true;
    }
}
