using Godot;
using System.Collections.Generic;

public class LevelGenerator
{
    int type;
    string argument;

    public LevelGenerator(int type, string argument)
    {
        this.type = type;
        this.argument = argument;
    }

    public void GenerateMap(Model model, List<ModelEvent> eventQueue)
    {
        if (type == 0)
        {
            EditorGenerator.GenerateMap(model, eventQueue, argument);
        }
    }

    public void GenerateEntities(Model model, List<ModelEvent> eventQueue)
    {
        if (type == 0)
        {
            EditorGenerator.GenerateEntities(model, eventQueue, argument);
        }
    }
}
