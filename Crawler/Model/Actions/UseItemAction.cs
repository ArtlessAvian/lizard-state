using System;
using Godot;
using Godot.Collections;

public class UseItemAction : Action
{
    InventoryItem item;
    Action proxyAction;

    public UseItemAction(Entity e, int id)
    {
        item = e.inventory;
        proxyAction = (Action)Activator.CreateInstance(Type.GetType(item.data.associatedAction));
        // proxyAction = new AttackAction(e, -1); // e.inventory.something()
    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        item.uses -= 1;
        proxyAction.Do(model, e);

        return true;
    }

    public override Action SetTarget((int x, int y) target)
    {
        return proxyAction.SetTarget(target);
    }

    public override Action SetTargetRelative((int x, int y) delta)
    {
        return proxyAction.SetTargetRelative(delta);
    }

    public override bool IsValid(Model model, Entity e)
    {
        if (item.uses <= 0) { return false; }
        return proxyAction.IsValid(model, e);
    }

    public override (float, float) Range => proxyAction.Range;
}
