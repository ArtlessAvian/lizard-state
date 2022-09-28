using Godot;
using Godot.Collections;

public class KnockdownWakeupAction : Action
{
    public override Dictionary Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return null;
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

        e.state = Entity.EntityState.OK;
        return CreateModelEvent(e.id, "Wakeup");
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }
}
