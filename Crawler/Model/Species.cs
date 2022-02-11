using Godot;
using System;
using System.Collections.Generic;

public class Species : Resource
{
    // Game Logic
    [Export] public AI ai = null;

    // [Export] public AttackData bumpAttack;
    [Export] public RushAttackData rushAttack;
    [Export] public List<ReachAttackData> attacks;
    [Export] public List<string> abilities;

    // Stat Block
    [Export] public int maxHealth = 30;
    
    // View stuff
    [Export] public string displayName;
}
