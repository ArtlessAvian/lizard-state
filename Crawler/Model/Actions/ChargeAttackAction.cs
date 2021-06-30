using Godot;
using System.Collections.Generic;

public class ChargeAttackAction : ActionTargeted
{
    AttackData data;

    public ChargeAttackAction(AttackData data = null)
    {
        this.data = data;
    }

    public override bool Do(ModelAPI api, Entity e)
    {
        if (this.data is null)
        {
            this.data = GD.Load<AttackData>("res://Crawler/Model/Attacks/BasicAttack.tres");
            GD.PrintErr($"{e.species.ResourcePath} missing attacks?");
        }

        if (e.energy < this.data.energy)
        {
            return false;
        }

        // TODO: unhardcode
        api.ApiEvent(new ModelEvent(-1, "Wait"));
        // api.ApiEvent(new ModelEvent(e.id, "Charge"));
        api.ApiEvent(new ModelEvent(-1, "Print", "Charging!!"));

        e.queuedAction = new AttackAction(data).SetTarget(GetTargetPos(e.position));
        e.nextMove += 10;

        api.ApiEvent(new ModelEvent(-1, "Wait"));
        return true;
    }
}
