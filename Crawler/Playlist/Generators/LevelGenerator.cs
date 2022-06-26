using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Helps fill out an empty model.
/// </summary>
public interface LevelGenerator
{
    Model Generate(Model model);
    void GenerateMap(Model model);
}
