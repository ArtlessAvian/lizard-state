using System;
using Godot;

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

    public FloorItem BuildFloorItem((int x, int y) position)
    {
        FloorItem floor = GD.Load<CSharpScript>("res://Crawler/Model/FloorItem.cs").New() as FloorItem;
        floor.position = position;
        floor.inventoryItem = this;
        return floor;
    }

    public Action BuildAction()
    {
        UseItemAction action = GD.Load<CSharpScript>("res://Crawler/Model/Actions/UseItemAction.cs").New() as UseItemAction;
        action.item = this;
        return action;
    }
}