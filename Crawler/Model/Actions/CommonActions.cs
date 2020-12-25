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
            if (e.team == entityAt.team)
            {
                entityAt.position.x = e.position.x;
                entityAt.position.y = e.position.y;

                eventQueue.Add(new ModelEvent(entityAt, "Moved", entityAt.position));
            }
            else
            {
                GD.Print($"{e.species.displayName} bumps into the {entityAt.species.displayName}");
                return false;
            }
        }

        e.position.x += displacement.x;
        e.position.y += displacement.y;
        e.nextMove += 1;

        eventQueue.Add(new ModelEvent(e, "Moved", e.position));

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
        if ((target is null) || target == e)
        {
            return false;
        }

        target.health -= 1;
        api.DisplayMessage($"{e.species.displayName} hits {target.species.displayName}!");

        e.nextMove += 1;

        return true;
    }
}
