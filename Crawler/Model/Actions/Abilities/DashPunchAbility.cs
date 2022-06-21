using Godot;
using System.Collections.Generic;

public class DashPunchAbility : Action
{
    public override bool Do(Model model, Entity e)
    {
        GD.Print("test");

        if (!IsValid(model, e)) { return false; }

        (int x, int y) targetPos = GetTargetPos(e.position);

        if (targetPos.x == e.position.x && targetPos.y == e.position.y)
        {
            return true;
        }

        (int x, int y) place = e.position;
        Entity target = null;
        foreach ((int x, int y) tile in GridHelper.RayThrough(e.position, targetPos))
        {
            if (tile != e.position)
            {
                if (GridHelper.Distance(tile, e.position) > 5) { break; }
                if ((target = model.GetEntityAt(tile)) != null) { break; }
                if (!model.CanWalkFromTo(place, tile)) { break; }
            }
            place = tile;
        }

        e.position = place;
        model.CoolerApiEvent(e.id, "Move", new Vector2(e.position.x, e.position.y));

        if (target != null)
        {
            // Action subaction = new RushAttackAction();
            // subaction.SetTarget(target.position);
            // subaction.Do(model, e);
        }

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }

    public override (int, int) Range => (1, 5);
    public override TargetingType.Type targetingType => new TargetingType.Ray(false, true);
}
