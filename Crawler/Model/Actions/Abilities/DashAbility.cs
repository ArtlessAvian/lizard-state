using Godot;
using System.Collections.Generic;

public class DashAbility : Action
{
    public DashAbility() 
    {

    }

    public override bool Do(ModelAPI api, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        if (GridHelper.Distance(e.position, targetPos) > 5)
        {
            return false;
        }

        // GridHelper.LineBetween(e.position, targetPos);

        e.position = targetPos;
        e.nextMove += 10;

        api.ApiEvent(new ModelEvent(e.id, "Move", e.position));

        return true;
    }

    public override (int, int) Range => (1, 5);
}
