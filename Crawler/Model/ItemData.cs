using Godot;

public class ItemData : Resource
{
    [Export]
    public string associatedAction = "DashAbility";

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