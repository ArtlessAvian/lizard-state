using Godot;
using System.Collections.Generic;

public class DashAbility : Action
{
    public DashAbility() 
    {

    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        (int x, int y) targetPos = GetTargetPos(e.position);

        e.position = targetPos;
        e.nextMove += 2;

        model.CoolerApiEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        if (GridHelper.Distance(e.position, targetPos) > Range.max)
        {
            return false;
        }

        return true;
    }

    public override (float min, float max) Range => (1, 5);
}
