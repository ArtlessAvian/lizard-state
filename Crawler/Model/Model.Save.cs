using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class Model
{
    public Dictionary SaveToDictionary()
    {
        Dictionary dict = new Dictionary();
        dict["time"] = time;
        dict["Entities"] = SaveEntities();
        GD.Print(dict);
        return dict;
    }

    Array SaveEntities()
    {
        Array dict = new Array();
        foreach (Entity e in entities)
        {
            dict.Add(e.SaveToDictionary());
        }
        return dict;
    }

    // void LoadFromDictionary(Dictionary dict)
    public Model(List<ModelEvent> eventQueue, Dictionary dict) : this(eventQueue)
    {
        this.time = (int)dict["time"];
        foreach (Dictionary entityDict in (Array)dict["Entities"])
        {
            this.AddEntity(eventQueue, new Entity(entityDict));
        }
    }
}
