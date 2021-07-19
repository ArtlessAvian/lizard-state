using Godot;
using System.Collections.Generic;

public class DashAbility : Action
{
    public DashAbility() 
    {

    }

    public override bool Do(ModelAPI api, Entity e)
    {
        if (!IsValid(api, e))
        {
            return false;
        }

        (int x, int y) targetPos = GetTargetPos(e.position);

        e.position = targetPos;
        e.nextMove += 10;

        api.ApiEvent(new ModelEvent(e.id, "Move", e.position));

        return true;
    }

    public override bool IsValid(ModelAPI api, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        if (GridHelper.Distance(e.position, targetPos) > 10)
        {
            return false;
        }

        return true;
    }

    public override (int min, int max) Range => (1, 10);
}
