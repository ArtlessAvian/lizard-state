using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// How the model communicates *what order things happen* to the view.
/// See NewEvent() in the ModelAPI interface.
/// </summary>
/// If the view peeked at the model, the model could be running ahead of the view.
/// It is OK to peek if the view is in sync (waiting on player input), but it (ideally) shouldn't need to.
public struct ModelEvent
{
    public int subject;
    public string action;
    // arg type can be inferred from action.
    // use like an adverb or adverb phrase!
    public object args; // can be null!
    public int obj; // "null" is -1

    public ModelEvent(int subject, string action, object args = null, int @object = -1)
    {
        this.subject = subject;
        this.action = action;
        this.args = args;
        this.obj = @object;
    }
}

/// <summary>
/// Represents something. [a trip somewhere and back, or a "run."]
/// Stores the game state and handles turn taking.
/// Remember to keep view information in the view counterpart!
/// </summary>
public partial class Model : Node
{
    // Everything is saved!!
    [Export] public int time = 0;

    public CrawlerMap Map
    {
        get { return GetNode<CrawlerMap>("Map"); }
    }

    public Node Entities
    {
        get { return GetNode("Entities"); } 
    }

    // given to model by generator
    public Dictionary generatorData;

    public delegate void EventHandler(ModelEvent ev);
    public EventHandler NewEvent;

    // public Model() {}

    public void AddEntity(Entity e)
    {
        e.id = Entities.GetChildCount();
        Entities.AddChild(e);

        this.NewEvent(new ModelEvent(-1, "Create", e, e.id));

        if (e.providesVision)
        {
            GetNode<VisionSystem>("Systems/Vision").UpdateVision(this, e);
            // this.NewEvent(new ModelEvent(e.id, "SeeMap", (e.position, Map.GetVisibleTiles(e.position, 5))));
        }
    }

    /// <summary>
    /// Sets the next action for the player character.
    /// <returns> true if its the player turn AND action is valid AND (reasonable OR forced)  </returns>
    /// </summary>
    // 
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
            NewEvent(new ModelEvent(e.id, "Unstun"));
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
            action = e.ai?.GetMove(this, e);
        }

        // If no action, return
        if (action == null)
        {
            if (!e.species.isPlayer) { GD.Print("no move!"); }
            return false;
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
        GetNode<VisionSystem>("Systems/Vision").Run(this);
    }

    private Entity NextEntity()
    {
        Entity result = GetEntity(0);
        foreach (Entity e in Entities.GetChildren())
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
}
