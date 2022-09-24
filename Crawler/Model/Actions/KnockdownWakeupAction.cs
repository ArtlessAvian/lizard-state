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
        // if no one is standing on you
        //     get up
        // else if random
        //     push the entity on you somewhere
        //     get up
        // else
        //     fail to get up
        //     queue another getup attempt
        //     repeat getup on next entity?

        model.CoolerApiEvent(e.id, "Wakeup");
        e.state = Entity.EntityState.OK;
        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }
}
