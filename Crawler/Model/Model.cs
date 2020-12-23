using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

// not saved
public struct ModelEvent
{
    public Entity subject;
    public string action; // Like an enum, but worse.
    public object args; // arg type can be inferred from action.

    public ModelEvent(Entity subject, string action, object args = null)
    {
        this.subject = subject;
        this.action = action;
        this.args = args;
    }
}

public partial class Model
{
    // Saved
    List<Entity> entities;
    public int time = 0;

    // Generated
    Dictionary generatorData;
    public TileMap map; // conveniently, also a Godot Tilemap.

    public Model(List<ModelEvent> eventQueue, Dictionary generatorData)
    {
        map = new TileMap();
        entities = new List<Entity>();
        this.generatorData = generatorData;
    }

    public void AddEntity(List<ModelEvent> eventQueue, Entity e)
    {
        entities.Add(e);
        eventQueue.Add(new ModelEvent(e, "Created"));
    }

    // returns true if successful
    public bool DoPlayerAction(List<ModelEvent> eventQueue, Action action)
    {
        Entity e = NextEntity();
        PassTime(e.nextMove);

        if (!e.species.isPlayer) { return false; }

        bool success = action.Do(this, eventQueue, e);
        if (!success)
        {
            GD.Print("Can't do that!");
            return false;
        }
        return true;
    }

    // returns false if its the player turn.
    public bool DoEntityAction(List<ModelEvent> eventQueue)
    {
        Entity e = NextEntity();
        PassTime(e.nextMove);

        if (e.species.isPlayer) { return false; }

        bool success = e.ai.GetMove(this, e).Do(this, eventQueue, e);
        if (!success)
        {
            GD.Print($"{e.species.displayName} made bad move. Skipping!");
            e.nextMove++;
        }

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
            if (e.nextMove < result.nextMove)
            {
                result = e;
            }
        }
        return result;
    }
}
