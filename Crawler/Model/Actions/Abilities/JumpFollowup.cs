using Godot;
using System.Collections.Generic;
using Godot.Collections;

// Neutral skip tool. Like a FF Dragoon jump.
public class JumpFollowup : Action
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        model.CoolerApiEvent(new Dictionary() {
                {"subject", e.id},
                {"action", "AttackActive"},
                {"args", new Vector2(e.position.x, e.position.y)},
                {"flavorTags", new Godot.Collections.Array{ "JumpLand" }}
            });
        if (model.GetEntityAt(e.position) is Entity trample)
        {
            trample.KnockdownForTurns(2, model.time, e.id);
            model.CoolerApiEvent(new Dictionary() {
                {"subject", e.id},
                {"action", "Hit"},
                {"object", trample.id},
                {"damage", 0},
                {"swept", true}
            });
        }
        foreach (Entity other in model.GetEntitiesInRadius(e.position, 1))
        {
            other.StunForTurns(1, model.time, e.id);
            model.CoolerApiEvent(new Dictionary() {
                {"subject", e.id},
                {"action", "Hit"},
                {"object", other.id},
                {"damage", 0},
                {"swept", false}
            });
        }

        e.state = Entity.EntityState.OK;
        e.nextMove += 1;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }

    public override (int min, int max) Range => (1, 1);
    public override TargetingType.Type TargetingType => new TargetingType.Cone { sectorDegrees = 360 };
}
