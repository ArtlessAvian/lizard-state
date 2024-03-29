using Godot;
using System;
using System.Collections.Generic;

// TODO: Replace with prototype pattern
public class Species : Resource
{
    // Game Logic
    [Export] public AI ai = null;

    // [Export] public AttackData bumpAttack;
    [Export] public RushAttackAction rushAttack;
    [Export] public List<Action> attacks;
    [Export] public List<Action> abilities;

    // Stat Block
    [Export] public int maxHealth = 30;

    // View stuff
    [Export] public string displayName;

    public Entity BuildEntity((int, int) position, int team)
    {
        Entity entity = (Entity)GD.Load<CSharpScript>("res://Crawler/Model/Entity.cs").New();
        entity.SetSpecies(this);
        entity.SetTeam(team);
        entity.position = position;
        return entity;
    }
}
