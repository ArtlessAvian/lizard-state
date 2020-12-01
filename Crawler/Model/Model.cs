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
        Species playerTegu = GD.Load<Resource>("res://Crawler/Model/Species/PlayerTegu.tres") as Species;

        entities = new List<Entity>();
        AddEntity(eventQueue, new Entity(playerTegu, (0, 0)));
        AddEntity(eventQueue, new Entity(playerTegu, (0, 1)));
        AddEntity(eventQueue, new Entity(playerTegu, (0, 2)));
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

            e.position.x = (int)(GD.Randi() % 7) - 3;
            e.position.y = (int)(GD.Randi() % 7) - 3;
            ev.action = "Moved";
            ev.args = $"{e.position.x},{e.position.y}";
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
