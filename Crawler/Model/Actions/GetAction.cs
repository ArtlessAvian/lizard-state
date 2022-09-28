using Godot;
using Godot.Collections;

public class GetAction : Action
{
    public override Dictionary Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return null;
        }

        if (model.GetMap().GetCell(e.position.x, e.position.y) == 2)
        {
            e.nextMove += 3;
            return CreateModelEvent(e.id, "Debug", "Got the moss!");
        }

        foreach (FloorItem floorItem in model.GetFloorItems())
        {
            if (floorItem.positionX == e.positionX && floorItem.positionY == e.positionY)
            {
                e.inventory = floorItem.inventoryItem ?? new InventoryItem(GD.Load<ItemData>("res://Crawler/Model/ItemData/Something.tres"));
                e.nextMove += 3;
                return CreateModelEvent(e.id, "Debug", $"{e.species.displayName} now holds the {e.inventory.data.ResourceName}.");
            }
        }

        return null;
    }

    public override bool IsValid(Model model, Entity e)
    {
        if (model.GetMap().GetCell(e.position.x, e.position.y) == 2)
        {
            return true;
        }

        foreach (FloorItem item in model.GetFloorItems())
        {
            if (item.positionX == e.positionX && item.positionY == e.positionY)
            {
                return true;
            }
        }

        return false;
    }
}
