using Godot;
using Godot.Collections;

public class StunRecoveryAction : Action
{
    public override Dictionary Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return null;
        }

        e.state = Entity.EntityState.OK;
        return CreateModelEvent(e.id, "Unstun");
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }
}
