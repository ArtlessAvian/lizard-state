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

    public Model Generate(Model model)
    {
        model.generatorData = this.SaveToDict();        
        
        // Assumes the map stays constant.
        GenerateMap(model);

        model.time = (int)dict["time"];
        foreach (Dictionary entityDict in (Array)dict["Entities"])
        {
            model.AddEntity(new Entity(entityDict));
        }

        return model;
    }

    public void GenerateMap(Model model)
    {
        this.generator.GenerateMap(model);
    }

    public Dictionary SaveToDict()
    {
        return generator.SaveToDict();
    }
}
