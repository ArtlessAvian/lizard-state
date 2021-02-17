using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

// not saved
public struct ModelEvent
{
    public int subject;
    public string action;
    // arg type can be inferred from action.
    // use like an adverb or adverb phrase!
    public object args;
    public int obj;

    public ModelEvent(int subject, string action, object args = null, int @object = -1)
    {
        this.subject = subject;
        this.action = action;
        this.args = args;
        this.obj = @object;
    }
}

public partial class Model
{
    // Everything is saved!!
    List<Entity> entities;
    public int time = 0;
    Dictionary generatorData;
    public Map map;

    private List<ModelEvent> eventQueue;

    public Model(List<ModelEvent> eventQueue, Dictionary generatorData)
    {
        map = new Map();
        entities = new List<Entity>();
        this.generatorData = generatorData;
        this.eventQueue = eventQueue;
    }

    public void AddEntity(Entity e)
    {
        e.id = entities.Count;
        entities.Add(e);
        eventQueue.Add(new ModelEvent(-1, "Create", e, e.id));

        if (e.providesVision)
        {
            NewEvent(new ModelEvent(-1, "UpdateVision", (e.position, map.GetVisibleTiles(e.position, 3)), e.id));
        }
    }

    // returns true if successful
    public bool DoPlayerAction(Action action)
    {
        Entity e = NextEntity();
        PassTime(e.nextMove);

        if (!e.species.isPlayer) { return false; }

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

    public void VisionEvent()
    {
        foreach (Entity e in entities)
        {
            if (e.dirtyVision)
            {
                NewEvent(new ModelEvent(-1, "UpdateVision", (e.position, map.GetVisibleTiles(e.position, 3)), e.id));
                e.dirtyVision = false;
            }
        }
    }

    // returns false if its the player turn.
    public bool DoEntityAction()
    {
        Entity e = NextEntity();
        PassTime(e.nextMove);

        if (e.species.isPlayer) { return false; }

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

    public void PassTime(int finalTime)
    {
        int delta = finalTime - time;
        time = finalTime;
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
        return result;
    }
}
