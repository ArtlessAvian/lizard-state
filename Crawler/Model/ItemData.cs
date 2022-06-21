using Godot;

public class ItemData : Resource
{
    [Export]
    public Action action; // duplicate me, don't use directly

    [Export]
    public int maxUses = 5;

    public ItemData()
    {
        if (ResourceName == "")
        {
            ResourceName = "Something";
        }
    }
}