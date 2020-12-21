using Godot;
using System;

public class Species : Resource
{
    [Export]
    public int maxHealth = 10;

    [Export]
    public string displayName;

    [Export]
    public bool isPlayer = false;

    [Export]
    public string aiType;
}
