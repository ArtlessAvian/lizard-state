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
    [Export] public List<string> abilities;

    // Stat Block
    [Export] public int maxHealth = 30;
    
    // View stuff
    [Export] public string displayName;

    public Entity CreateEntity((int x, int y) position, int team)
    {
        Entity entity = (Entity)GD.Load<CSharpScript>("res://Crawler/Model/Entity.cs").New();
        entity.Construct(this, position, team);
        return entity;
    }
}
