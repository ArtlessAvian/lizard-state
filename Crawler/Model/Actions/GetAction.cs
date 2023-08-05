using Godot;
using System;
using System.Collections.Generic;

public class GetAction : CrawlAction
{
    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        if (model.GetMap().tiles.GetCell(e.position.x, e.position.y) == 2)
        {
            model.CoolerApiEvent(-1, "Print", "Got the moss.");
            e.nextMove += 3;
            return true;
        }

        FloorItem floorItem = null;
        foreach (FloorItem item in model.GetFloorItems())
        {
            if (item.positionX == e.positionX && item.positionY == e.positionY && item.inventoryItem != null)
            {
                floorItem = item;
                break;
            }
        }

        if (floorItem != null)
        {
            if (floorItem.inventoryItem != null)
            {
                e.inventory.Add(floorItem.inventoryItem);
                floorItem.inventoryItem = null; // avoid shared reference?
            }
            else
            {
                ItemData data = GD.Load<ItemData>("res://Crawler/Model/ItemData/Something.tres");
                InventoryItem item = data.BuildInventoryItem();
                e.inventory.Add(item);
            }
            // TODO: Delete the floor item.
            model.CoolerApiEvent(e.id, "GetItem", floorItem.id);

            // model.CoolerApiEvent(-1, "Debug", $"{e.species.displayName} got the {e.inventory.data.ResourceName}.");
            e.nextMove += 3;
            return true;
        }

        return false;
    }

    public override bool IsValid(Model model, Entity e)
    {
        if (model.GetMap().tiles.GetCell(e.position.x, e.position.y) == 2)
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
