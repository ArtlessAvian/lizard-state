using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Builds a model from scratch. A model factory?
/// </summary>
public interface LevelGenerator
{
    Model Generate(List<ModelEvent> eventQueue);
    void GenerateMap(Model model, List<ModelEvent> eventQueue);
    Dictionary SaveToDict();
}
