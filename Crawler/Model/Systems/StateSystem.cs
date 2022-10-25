using System;
using Godot;
using Godot.Collections;

// Handles knockdown (health > 0), and "down" down (health <= 0).
public class StateSystem : Resource, CrawlerSystem
{
    readonly CSharpScript inventoryItemScript;
    readonly CSharpScript floorItemScript;
    public StateSystem()
    {
        inventoryItemScript = GD.Load<CSharpScript>("res://Crawler/Model/InventoryItem.cs");
        floorItemScript = GD.Load<CSharpScript>("res://Crawler/Model/FloorItem.cs");
    }

    public void ProcessEvent(Model model, Dictionary ev) { }

    public void Run(Model model)
    {
        // down everyone <= 0 hp.
        foreach (Entity e in model.GetEntities())
        {
            if (e.state == Entity.EntityState.UNALIVE) { continue; }

            if (e.health <= 0)
            {
                e.state = Entity.EntityState.UNALIVE;
                e.queuedAction = null;
                model.CoolerApiEvent(e.id, "Downed");

                if (e.id == 0)
                {
                    model.done = true;
                }

                ItemData data = (ItemData)GD.Load("res://Crawler/Model/ItemData/Something.tres");
                InventoryItem item = (InventoryItem)inventoryItemScript.New(data);
                FloorItem floorItem = (FloorItem)floorItemScript.New();
                floorItem.inventoryItem = item;
                floorItem.position = e.position;
                model.AddFloorItem(floorItem);
            }
        }
    }
}