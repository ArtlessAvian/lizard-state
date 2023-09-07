using System;
using Godot;
using Godot.Collections;

namespace LizardState.Engine
{
    public class UseItemAction : CrawlAction
    {
        public InventoryItem item;
        CrawlAction ProxyAction
        {
            get
            {
                if (_proxyAction is null)
                {
                    _proxyAction = item.data.action.Duplicate() as CrawlAction;
                }
                return _proxyAction;
            }
        }
        private CrawlAction _proxyAction = null;

        public static UseItemAction New(InventoryItem item)
        {
            UseItemAction instance = (UseItemAction)GD.Load<CSharpScript>("res://Engine/Actions/UseItemAction.cs").New();
            instance.item = item;
            return instance;
        }

        public override bool Do(Model model, Entity e)
        {
            ProxyAction.SetTarget(GetTargetPos(e.position));

            if (!IsValid(model, e))
            {
                return false;
            }

            item.uses -= 1;
            ProxyAction.Do(model, e);

            return true;
        }

        public override bool IsValid(Model model, Entity e)
        {
            if (item.uses <= 0) { return false; }
            return ProxyAction.IsValid(model, e);
        }

        public override (int, int) Range => ProxyAction.Range;
        public override TargetingType.Type TargetingType => ProxyAction.TargetingType;
    }
}
