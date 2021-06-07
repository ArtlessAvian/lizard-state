using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class LoadedGenerator : LevelGenerator
{
    Dictionary dict;
    LevelGenerator generator;

    public LoadedGenerator(Dictionary dict)
    {
        this.dict = dict;
        Dictionary data = (Dictionary)dict["generatorData"];
        switch ((string)data["Type"])
        {
            case "Editor":
                this.generator = new EditorGenerator(data);
                break;
            case "Noise":
                this.generator = new NoiseGenerator();
                break;
        }
    }

    public Model Generate(List<ModelEvent> eventQueue)
    {
        Model model = new Model(eventQueue, this.SaveToDict());
        
        GenerateMap(model, eventQueue);

        model.time = (int)dict["time"];
        foreach (Dictionary entityDict in (Array)dict["Entities"])
        {
            model.AddEntity(new Entity(entityDict));
        }

        return model;
    }

    public void GenerateMap(Model model, List<ModelEvent> eventQueue)
    {
        this.generator.GenerateMap(model, eventQueue);
    }

    public Dictionary SaveToDict()
    {
        return generator.SaveToDict();
    }
}
