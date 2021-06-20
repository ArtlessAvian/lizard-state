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
        dict["generatorData"] = generatorData;
        // GD.Print(dict);
        return dict;
    }

    Array SaveEntities()
    {
        Array dict = new Array();
        foreach (Entity e in Entities.GetChildren())
        {
            dict.Add(e.SaveToDictionary());
        }
        return dict;
    }

    // To load, use the LoadedGenerator.
}
