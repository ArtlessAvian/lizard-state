using Godot;
using System;
using System.Collections.Generic;

// not saved
public struct ModelEvent
{
    public Entity subject;
    public String action; // Like an enum, but worse.
    public object args; // arg type can be inferred from action.
}

public class Model
{
    List<Entity> entities;
    int time = 0;

    public Model(List<ModelEvent> eventQueue)
    {
        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;
        Species partnerAxolotl = GD.Load<Resource>("res://Crawler/Model/Species/PartnerAxolotl.tres") as Species;

        entities = new List<Entity>();
        AddEntity(eventQueue, new Entity(playerTegu, (0, 0)));
        AddEntity(eventQueue, new Entity(partnerAxolotl, (0, 1)));
        AddEntity(eventQueue, new Entity(partnerAxolotl, (0, 2)));
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

        action.Do(eventQueue, e);
    }

    // returns false if its the player turn.
    public bool DoEntityAction(List<ModelEvent> eventQueue)
    {
        Entity e = NextEntity();
        PassTime(e.nextMove);

        if (e.species.isPlayer) { return false; }

        DebugAction(eventQueue, e);

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

    void SaveFromDictionary()
    {

    }

    void LoadFromDictionary()
    {
        
    }
}
