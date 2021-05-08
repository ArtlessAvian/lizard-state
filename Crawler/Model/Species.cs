using Godot;
using System;
using System.Collections.Generic;

public class Species : Resource
{
    // Game Logic
    [Export] public bool isPlayer = false;
    [Export] public string aiType;

    [Export] public AttackData bumpAttack;
    [Export] public List<AttackData> attacks;

    // Stat Block
    [Export] public int maxHealth = 30;
    
    // View stuff
    [Export] public string displayName;
}
