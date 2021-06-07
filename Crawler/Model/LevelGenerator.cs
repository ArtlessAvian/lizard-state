using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Builds a model from scratch. A model factory?
/// </summary>
public interface LevelGenerator
{
    Model Generate(Model model);
    void GenerateMap(Model model);
    Dictionary SaveToDict();
}
