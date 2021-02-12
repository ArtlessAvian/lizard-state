using Godot;
using System;

public class AttackData : Resource
{
    [Export] public float comboStartChance;
    [Export] public float comboLinkChance;
    [Export] public int hitDamage;
    [Export] public int missDamage;
    [Export] public int stun;
}
