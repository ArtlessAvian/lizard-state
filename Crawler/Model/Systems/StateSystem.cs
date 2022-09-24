using System;
using Godot;
using Godot.Collections;

// Handles knockdown (health > 0), and "down" down (health <= 0).
public class StateSystem : Node, CrawlerSystem
{
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
            }
        }
    }
}