using Godot;
using System;
using System.Collections.Generic;

public class GetAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        if (model.GetMap().GetCell(e.position.x, e.position.y) == 2)
        {
            model.CoolerApiEvent(-1, "Print", "Got the moss.");
            e.nextMove += 3;
            return true;
        }

        foreach (FloorItem floorItem in model.GetFloorItems())
        {
            if (floorItem.positionX == e.positionX && floorItem.positionY == e.positionY)
            {
                model.CoolerApiEvent(-1, "Debug", "Got the thingy.");
                e.inventory = floorItem.inventoryItem ?? new InventoryItem(GD.Load<ItemData>("res://Crawler/Model/ItemData/Something.tres"));
                model.CoolerApiEvent(-1, "Debug", $"{e.species.displayName} now holds the {e.inventory.data.ResourceName}.");
                e.nextMove += 3;
                return true;
            }
        }

        return false;
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
