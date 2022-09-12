using System;
using Godot;
using Godot.Collections;

public class DownedSystem : Node, CrawlerSystem
{
    public void ProcessEvent(Model model, Dictionary ev) { }

    public void Run(Model model)
    {
        foreach (Entity e in model.GetEntities())
        {
            if (e.downed) { continue; }

            if (e.health <= 0)
            {
                e.downed = true;
                e.nextMove = -1;
                e.queuedAction = null;
                model.CoolerApiEvent(e.id, "Downed");
            }
        }
    }
}