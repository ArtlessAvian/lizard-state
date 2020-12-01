using Godot;
using System;
using System.Collections.Generic;

// not saved
public struct ModelEvent
{
    public Entity subject;
    public String action;
    public String args;
}

public class Model
{
    List<Entity> entities;

    public Model(List<ModelEvent> eventQueue)
    {
        entities = new List<Entity>();
        AddEntity(eventQueue, new Entity());
        AddEntity(eventQueue, new Entity());
        AddEntity(eventQueue, new Entity());
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

    public void Tick(List<ModelEvent> eventQueue)
    {
        foreach (Entity e in entities)
        {
            e.health -= 3;
            ModelEvent ev;
            ev.subject = e;
            ev.action = "Damaged";
            ev.args = "";
            eventQueue.Add(ev);
        }
    }

    void SaveFromDictionary()
    {

    }

    void LoadFromDictionary()
    {
        
    }
}
