using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class LoadedGenerator : LevelGenerator
{
    LevelGenerator generator;

    public LoadedGenerator(Dictionary dict)
    {
        string type = (string)dict["Type"];
        switch (type)
        {
            case "Editor":
                generator = new EditorGenerator((string)dict["ScenePath"]);
                break;
        }
    }

    public void GenerateMap(Model model, List<ModelEvent> eventQueue)
    {
        generator.GenerateMap(model, eventQueue);
    }

    public void GenerateEntities(Model model, List<ModelEvent> eventQueue)
    {
        generator.GenerateEntities(model, eventQueue);
    }

    public Dictionary SaveToDict()
    {
        return generator.SaveToDict();
    }
}
