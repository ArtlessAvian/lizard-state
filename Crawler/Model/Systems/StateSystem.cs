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
                e.nextMove = -1;
                e.queuedAction = null;
                model.CoolerApiEvent(e.id, "Downed");
            }
        }

        // heal downed player characters.
        foreach (Entity e in model.GetEntities())
        {
            if (e.state == Entity.EntityState.UNALIVE && e.team == 0 && e.health <= 0)
            {
                // and if no enemies are around.
                // heal to one.
                e.health = 1;
                e.DelayNextMove(0, model.time, 0);
            }
        }

        Entity next = model.NextEntity();
        if (next.state == Entity.EntityState.UNALIVE && next.health > 0)
        {
            next.state = Entity.EntityState.OK;
            model.CoolerApiEvent(next.id, "Wakeup");
            GD.Print("heya");
            // if no one is standing on you
            //     get up
            // else if random
            //     push the entity on you somewhere
            //     get up
        }
    }
}