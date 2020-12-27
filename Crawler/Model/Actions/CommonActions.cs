using Godot;
using System;
using System.Collections.Generic;

public class MoveAction : Action
{
    (int x, int y) displacement;

    public MoveAction(object args) 
    {
        displacement = ((int, int))args;
    }

    public bool Do(ModelAPI api, List<ModelEvent> eventQueue, Entity e)
    {
        if (!api.CanWalkFromTo(0, 0, e.position.x + displacement.x, e.position.y + displacement.y))
        {
            return false;
        }
        Entity entityAt = api.GetEntityAt(e.position.x + displacement.x, e.position.y + displacement.y);
        if (!(entityAt is null) && entityAt != e)
        {
            if (e.team != entityAt.team)
            {
                GD.Print($"{e.species.displayName} bumps into the {entityAt.species.displayName}");
                return false;
            }
            else
            {
                entityAt.position.x = e.position.x;
                entityAt.position.y = e.position.y;

                e.position.x += displacement.x;
                e.position.y += displacement.y;
                e.nextMove += 10;

                eventQueue.Add(new ModelEvent(null, "Wait"));
                eventQueue.Add(new ModelEvent(e, "Swap", entityAt.position, entityAt));
                eventQueue.Add(new ModelEvent(null, "Wait"));
                return true;
            }
        }
        // else
        e.position.x += displacement.x;
        e.position.y += displacement.y;
        e.nextMove += 10;

        eventQueue.Add(new ModelEvent(e, "Move", e.position));
        return true;
    }
}

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
            target.nextMove += 10;
        }

        eventQueue.Add(new ModelEvent(null, "Wait"));
        eventQueue.Add(new ModelEvent(null, "Print", $"{e.species.displayName} hits {target.species.displayName}!"));
        if (crit)
            eventQueue.Add(new ModelEvent(null, "Print", $"{target.species.displayName} is stunned!!"));

        eventQueue.Add(new ModelEvent(e, "Attack", damage, target));
        
        eventQueue.Add(new ModelEvent(null, "Wait"));

        return true;
    }
}
