public class InventoryItem
{
    public ItemData data;
    public int uses = -1;

    public InventoryItem(ItemData data)
    {
        this.data = data;
        uses = data.maxUses;
    }

    public InventoryItem() : this(new ItemData()) {}
}