using Godot;
using Godot.Collections;
using System.Collections.Generic;

public interface LevelGenerator
{
    Model Generate(List<ModelEvent> eventQueue);
    void GenerateMap(Model model, List<ModelEvent> eventQueue);
    Dictionary SaveToDict();
}
