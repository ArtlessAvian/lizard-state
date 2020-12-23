using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

// not saved
public struct ModelEvent
{
    public Entity subject;
    public String action; // Like an enum, but worse.
    public object args; // arg type can be inferred from action.
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

        ModelEvent ev;
        ev.subject = e;
        ev.action = "Created";
        ev.args = "";
        eventQueue.Add(ev);
    }

    public void DoPlayerAction(List<ModelEvent> eventQueue, Action action)
    {
        Entity e = NextEntity();
        PassTime(e.nextMove);

        if (!e.species.isPlayer) { return; }

        bool success = action.Do(this, eventQueue, e);
        if (!success)
        {
            GD.Print("Womp womp.");
            e.nextMove++;
        }
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
            GD.Print("Womp womp.");
            e.nextMove++;
        }

        return true;
    }

    public void DebugAction(List<ModelEvent> eventQueue, Entity e)
    {
        ModelEvent ev;
        ev.subject = e;

        e.health -= 3;
        ev.action = "Damaged";
        ev.args = null;
        eventQueue.Add(ev);

        e.position.x += (int)(GD.Randi() % 3) - 1;
        e.position.y += (int)(GD.Randi() % 3) - 1;
        e.nextMove += 1;
        
        ev.action = "Moved";
        ev.args = (e.position.x, e.position.y);
        eventQueue.Add(ev);
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
