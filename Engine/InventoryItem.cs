using System;
using Godot;

namespace LizardState.Engine
{
    public class InventoryItem : Resource
    {
        [Export] public ItemData data;
        [Export] public int uses = -1;

        public static InventoryItem New(ItemData data)
        {
            InventoryItem instance = (InventoryItem)GD.Load<CSharpScript>("res://Engine/InventoryItem.cs").New();
            instance.data = data;
            instance.uses = data.maxUses;
            return instance;
        }

        [Obsolete]
        public InventoryItem(ItemData data)
        {
            this.data = data;
            uses = data.maxUses;
        }

        private InventoryItem() { }

        [Obsolete]
        public FloorItem BuildFloorItem(AbsolutePosition position)
        {
            FloorItem floor = FloorItem.New(position, this);
            return floor;
        }

        [Obsolete]
        public CrawlAction BuildAction()
        {
            UseItemAction action = GD.Load<CSharpScript>("res://Engine/Actions/UseItemAction.cs").New() as UseItemAction;
            action.item = this;
            return action;
        }
    }
}