using Godot;
using System;
using System.Collections.Generic;

public class StunRecoveryAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        model.CoolerApiEvent(e.id, "Unstun");
        e.state = Entity.EntityState.OK;
        model.Debug("Unstunned!!!!!!!!!!!!!!!!!!!!!!");
        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }
}
