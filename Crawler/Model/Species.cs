using Godot;
using System;

public class Species : Resource
{
    // Game Logic
    [Export] public bool isPlayer = false;
    [Export] public string aiType;

    [Export] public AttackData bumpAttack;

    // Stat Block
    [Export] public int maxHealth = 30;
    
    // View stuff
    [Export] public string displayName;
}
