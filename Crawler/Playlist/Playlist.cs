using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// A sequence of floors. Defined by a sequence of generators.
/// References to old and current models are saved here too.
/// Generators can then access them.
/// </summary>
public class Playlist : Resource
{
    [Export] Array<LevelGenerator> generators;
    [Export] Array<Model> models;
    [Export] int current = -1;

    public Model NextModel()
    {
        if (current >= generators.Count - 1)
        {
            return null;
        }
        current += 1;
        Model model = GD.Load<PackedScene>("res://Crawler/Model/Model.tscn").Instance<Model>();

        return model;
    }
}