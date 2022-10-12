using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Helps fill out an empty model.
/// </summary>
public abstract class LevelGenerator : Resource
{
    public abstract Model Generate(Model model);
    public abstract void GenerateMap(Model model);
}
