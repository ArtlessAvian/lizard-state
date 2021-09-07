using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents something. [a trip somewhere and back, or a "run."]
/// Stores the game state and handles turn taking.
/// Remember to keep view information in the view counterpart!
/// </summary>
public partial class Model
{
    // Everything is saved!!
    [Export] public int time = 0;

    public CrawlerMap map = new CrawlerMap();
    public List<Entity> entities = new List<Entity>();
    public List<CrawlerSystem> systems = new List<CrawlerSystem>();

    // given to model by generator
    public Dictionary generatorData;

    // [Signal]
    public delegate void EventHandler(Dictionary ev);
    public EventHandler NewEvent;

    public Model()
    {
        systems.Add(new VisionSystem(map));
    }

    public void AddEntity(Entity e)
    {
        e.id = entities.Count;
        entities.Add(e);

        this.CoolerApiEvent(-1, "Create", e, e.id);

        if (e.providesVision)
        {
            GetSystem<VisionSystem>().UpdateVision(this, e);
            // this.NewEvent(new ModelEvent(e.id, "SeeMap", (e.position, Map.GetVisibleTiles(e.position, 5))));
        }
    }

    /// <summary>
    /// Sets the next action for the player character.
    /// <returns> true if its the player turn AND action is valid AND (reasonable OR forced)  </returns>
    /// </summary>
    // I wrote this to avoid code duplication between player and entity actions.
    // I don't think I did that correctly.
    public bool SetPlayerAction(Action action, bool force = false)
    {
        Entity e = NextEntity();
        if (!e.species.isPlayer) { return false; }

        if (!action.IsValid(this, e)) { return false; }
        // if (!action.IsReasonable() && !force) { return false; }

        e.queuedAction = action;
        return true;
    }

    /// <summary>
    /// Attempts to run the next entity's action.
    /// <returns> true if entity has action </returns>
    /// </summary>
    // 
    public bool DoStep()
    {
        Entity e = NextEntity();

        if (e.stunned)
        {
            this.CoolerApiEvent(e.id, "Unstun");
            e.ResetCombo();
        }

        // Get the next action.
        Action action = null;        
        // Always get the queued action.
        if (e.queuedAction != null)
        {
            action = e.queuedAction;
            e.queuedAction = null;
        }
        
        // If no action, get the ai's action.
        if (action == null)
        {
            action = e.species?.ai?.GetMove(this, e);
        }

        // If no action, return
        if (action == null)
        {
            if (!e.species.isPlayer) {
                GD.PrintErr($"{e.species.displayName} has no ai or ai returned null!");
                GD.PrintErr("waiting instead..");
                action = new MoveAction().SetTargetRelative((0, 0));
            }
            else
            {
                CoolerApiEvent(e.id, "YourTurn");
                return false;
            }
        }

        bool success = action.Do(this, e);
        if (!success)
        {
            GD.Print($"{e.species.displayName} tried {action.GetType().ToString()} and failed!");
            e.nextMove += 10;
        }

        RunSystems();
        return true;
    }

    /// <summary>
    /// Runs all the systems, (usually after every move).
    /// This could be more efficient but whatever.
    /// </summary>
    private void RunSystems()
    {
        GetSystem<VisionSystem>().Run(this);
    }

    private Entity NextEntity()
    {
        Entity result = GetEntity(0);
        foreach (Entity e in entities)
        {
            if (e.nextMove == -1) { continue; }
            if (e.nextMove < result.nextMove)
            {
                result = e;
            }
        }
        PassTime(result.nextMove);
        return result;
    }

    public void PassTime(int finalTime)
    {
        int delta = finalTime - time;
        time = finalTime;
    }

    // todo: rename this lmao
    // [Obsolete]
    public void CoolerApiEvent(int subject, string action, object args = null, int @object = -1)
    {
        CoolerApiEvent(new Godot.Collections.Dictionary()
        {
            {"subject", subject},
            {"action", action},
            {"args", args},
            {"object", @object}
        });
    }

    public void CoolerApiEvent(Godot.Collections.Dictionary @event)
    {
        @event.Add("timestamp", time);

        // For each system, decorate the event.
        // foreach (CrawlerSystem system in GetNode("Systems").GetChildren())
        // {
        //     system.ProcessEvent(this, @event);
        // }

        // Send the event to the view, if the player('s team) sees it.
        this.NewEvent(@event);

        // For each system, react to the event.
        // (Skill procs, or something? could be fun)
        foreach (CrawlerSystem system in systems)
        {
            system.ProcessEvent(this, @event);
        }
    }
}
