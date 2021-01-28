using Godot;
using System.Collections.Generic;

public class SpinAbility : Action
{
    public SpinAbility() 
    {

    }

    public bool Do(ModelAPI api, List<ModelEvent> eventQueue, Entity e)
    {
        e.nextMove += 10;

        List<Entity> targets = api.GetEntitiesInRadius(e.position.x, e.position.y, 1);
        
        eventQueue.Add(new ModelEvent(null, "Wait"));

        foreach (Entity target in targets)
        {
            if (target.team == e.team) {continue;}

            AttackAction.AttackResult roll = new AttackAction.AttackResult();
            roll.crit = false;
            roll.damage = 5;
        
            target.TakeDamage(5, e.nextMove);

            eventQueue.Add(new ModelEvent(null, "Print", $"{e.species.displayName} hits {target.species.displayName}!"));
            eventQueue.Add(new ModelEvent(e, "Attack", roll, target));
        
            if (target.downed)
                eventQueue.Add(new ModelEvent(null, "Print", $"{target.species.displayName} is downed!!"));
        }

        eventQueue.Add(new ModelEvent(null, "Wait"));

        return true;
    }
}
