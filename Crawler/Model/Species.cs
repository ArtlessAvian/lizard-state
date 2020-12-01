using Godot;
using System;

public class Species : Resource
{
    [Export]
    public int maxHealth = 10;

    [Export]
    public string displayName;
}
