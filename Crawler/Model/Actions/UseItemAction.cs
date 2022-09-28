using System;
using Godot;
using Godot.Collections;

public class UseItemAction : Action
{
    public InventoryItem item;
    Action ProxyAction
    {
        get
        {
            if (_proxyAction is null)
            {
                _proxyAction = item.data.action.Duplicate() as Action;
            }
            return _proxyAction;
        }
    }
    private Action _proxyAction = null;

    public override Dictionary Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return null;
        }

        Dictionary result = ProxyAction.Do(model, e);
        if (result is object)
        {
            item.uses -= 1;
        }
        return result;
    }

    public override Action SetTarget((int x, int y) target)
    {
        return ProxyAction.SetTarget(target);
    }

    public override Action SetTargetRelative((int x, int y) delta)
    {
        return ProxyAction.SetTargetRelative(delta);
    }

    public override bool IsValid(Model model, Entity e)
    {
        if (item.uses <= 0) { return false; }
        return ProxyAction.IsValid(model, e);
    }

    public override (int, int) Range => ProxyAction.Range;
    public override TargetingType.Type TargetingType => ProxyAction.TargetingType;
}
