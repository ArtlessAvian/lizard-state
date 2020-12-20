using Godot;
using Godot.Collections;
using System.Collections.Generic;

public interface LevelGenerator
{
    void GenerateMap(Model model, List<ModelEvent> eventQueue);
    void GenerateEntities(Model model, List<ModelEvent> eventQueue);
    Dictionary SaveToDict();
}
