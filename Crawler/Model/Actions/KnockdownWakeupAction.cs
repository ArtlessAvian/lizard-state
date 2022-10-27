using Godot;
using System;
using System.Collections.Generic;

public class KnockdownWakeupAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        // replace with following logic.
        bool success = false;
        if (model.GetEntityAt(e.position) is null)
        {
            success = true;
        }
        // else if random
        //     push the entity on you somewhere
        //     get up
        // else
        //     fail to get up

        if (success)
        {
            model.CoolerApiEvent(e.id, "Wakeup");
            e.state = Entity.EntityState.OK;
        }
        else
        {
            e.nextMove = model.time + 1;
        }
        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }
}
