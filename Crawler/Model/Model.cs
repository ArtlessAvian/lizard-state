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
/// Represents a trip somewhere and back, or a "run."
/// Stores the game state and handles turn taking.
/// Remember to keep view information in the view counterpart!
/// </summary>
public partial class Model : Node
{
    // Everything is saved!!
    [Export] public int time = 0;

    List<Entity> entities;
    public Map map;

    // given to model by generator
    public Dictionary generatorData;
    public List<ModelEvent> eventQueue;

    public Model()
    {
        map = new Map();
        entities = new List<Entity>();
        eventQueue = new List<ModelEvent>();
    }

    public void AddEntity(Entity e)
    {
        e.id = entities.Count;
        entities.Add(e);
        this.GetNode("Entities").AddChild(e);
        eventQueue.Add(new ModelEvent(-1, "Create", e, e.id));

        if (e.providesVision)
        {
            NewEvent(new ModelEvent(e.id, "SeeMap", (e.position, map.GetVisibleTiles(e.position, 5))));
        }
    }

    /// <summary>
    /// Attempts to run an action for the player.
    /// <returns> true if successful, false if not the player's turn or if action fails </returns>
    /// </summary>
    // TODO: Rework return type. (false means too many things!)
    public bool DoPlayerAction(Action action)
    {
        Entity e = NextEntity();
        if (!e.species.isPlayer) { return false; }

        if (e.stunned) { NewEvent(new ModelEvent(e.id, "Unstun")); }
        e.ResetCombo();
        
        bool success = action.Do(this, e);
        if (!success)
        {
            eventQueue.Add(new ModelEvent(-1, "Print", "Can't do that!"));
            return false;
        }
        
        VisionEvent();
        return true;
    }

    /// <summary>
    /// Runs the next entity's move.
    /// Whoever owns the model should run this in a loop until it returns false.
    /// </summary>
    /// <returns> false if its the player's turn. </returns>
    public bool DoEntityAction()
    {
        Entity e = NextEntity();
        if (e.species.isPlayer) { return false; }

        if (e.stunned) { NewEvent(new ModelEvent(e.id, "Unstun")); }
        e.ResetCombo();

        bool success = e.ai.GetMove(this, e).Do(this, e);
        if (!success)
        {
            GD.Print($"{e.species.displayName} made bad move. Skipping!");
            e.nextMove += 10;
        }
        
        VisionEvent();
        return true;
    }

    /// <summary>
    /// Runs whenever something providing vision could have moved.
    /// (For now, that's every time after anyone moves.)
    /// </summary>
    private void VisionEvent()
    {
        foreach (Entity e in entities)
        {
            if (e.dirtyVision)
            {
                NewEvent(new ModelEvent(e.id, "SeeMap", (e.position, map.GetVisibleTiles(e.position, 5))));
                e.dirtyVision = false;
            }
        }
    }

    private Entity NextEntity()
    {
        Entity result = entities[0];
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
}
