using Godot;
using System;

public class Species : Resource
{
    // Game Logic
    [Export] public bool isPlayer = false;
    [Export] public string aiType;

    // Stat Block
    [Export] public int maxHealth = 30;
    
    // View stuff
    [Export] public string displayName;
}
