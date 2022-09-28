using Godot;
using Godot.Collections;

public class DashAbility : Action
{
    public DashAbility()
    {

    }

    public override Dictionary Do(Model model, Entity e)
    {
        if (!IsValid(model, e)) { return null; }

        (int x, int y) targetPos = GetTargetPos(e.position);

        e.position = targetPos;
        e.nextMove += 2;

        return CreateModelEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));
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

    public override (int min, int max) Range => (1, 5);
}
