using Godot;
using Godot.Collections;

public class MoveAction : Action
{
    public override Dictionary Do(Model model, Entity e)
    {
        if (!IsValid(model, e)) { return null; }

        (int x, int y) targetPos = GetTargetPos(e.position);

        if (targetPos.x == e.position.x && targetPos.y == e.position.y)
        {
            return DoNothing(model, e);
        }

        Entity entityAt = model.GetEntityAt(targetPos);
        if (!(entityAt is null))
        {
            if (entityAt.team != e.team)
            {
                e.nextMove += 1;
                return CreateModelEvent(e.id, "Bump", entityAt.id);
            }
            else
            {
                return DoSwap(model, e, entityAt);
            }
        }

        return DoMove(model, e);
    }

    private Dictionary DoNothing(Model model, Entity e)
    {
        e.nextMove += 1;
        return CreateModelEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));
    }

    private Dictionary DoMove(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        e.nextMove += (int)(1 * GridHelper.Distance(e.position, targetPos));
        e.position = targetPos;

        return CreateModelEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));
    }

    private Dictionary DoSwap(Model model, Entity e, Entity teammate)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        teammate.position = e.position;
        e.position = targetPos;
        e.nextMove += 1;

        return CreateModelEvent(e.id, "Swap", new Vector2(teammate.position.x, teammate.position.y), teammate.id);
    }

    public override bool IsValid(Model model, Entity e)
    {
        // TODO: This one is tough. Usually true. (See MoveOrAttackAction.cs too).

        (int x, int y) targetPos = GetTargetPos(e.position);
        if (GridHelper.Distance(e.position, GetTargetPos(e.position)) > 1.5f) { return false; }
        if (!model.CanWalkFromTo(e.position, targetPos)) { return false; }
        return true;
    }

    public override (int, int) Range => (1, 1);
}
