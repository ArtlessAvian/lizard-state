using System;
using Godot;

namespace LizardState.Engine
{
    public class InventoryItem : Resource
    {
        [Export] public ItemData data;
        [Export] public int uses = -1;

        public InventoryItem(ItemData data)
        {
            this.data = data;
            uses = data.maxUses;
        }

        public InventoryItem() { }

        public FloorItem BuildFloorItem(AbsolutePosition position)
        {
            FloorItem floor = GD.Load<CSharpScript>("res://Engine/FloorItem.cs").New() as FloorItem;
            floor.position = position;
            floor.inventoryItem = this;
            return floor;
        }

        public CrawlAction BuildAction()
        {
            UseItemAction action = GD.Load<CSharpScript>("res://Engine/Actions/UseItemAction.cs").New() as UseItemAction;
            action.item = this;
            return action;
        }
    }
}