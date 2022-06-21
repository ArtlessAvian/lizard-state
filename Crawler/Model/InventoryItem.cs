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
}