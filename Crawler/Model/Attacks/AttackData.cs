using Godot;
using Godot.Collections;
using System;

public struct AttackResult
{
    public bool hit;
    public bool stuns;
    public int damage;
    public int stunUntil;

    public Dictionary ToDict()
    {
        return new Dictionary{{"hit", hit}, {"stuns", stuns}, {"damage", damage}, {"stunUntil", stunUntil}};
    }
}

public abstract class AttackData : Resource
{
    [Export] public float range = 1.5f;
    [Export] public int energy = 0;
    [Export] public int recovery = 10;

    [Export] public float comboStartChance;
    [Export] public float comboLinkChance;
    [Export] public int hitDamage = 1;
    [Export] public int chipDamage = 0;
    [Export] public int stun;

    [Export] public bool missable = false; // Display "miss" instead of "-0" 

    public AttackResult DoAttack(Entity target, int timeNow)
    {
        AttackResult result;
        result.stuns = GD.Randf() < (target.stunned ? comboLinkChance : comboStartChance);
        result.damage = result.stuns ? hitDamage : chipDamage;
        result.stunUntil = result.stuns ? timeNow + stun : 0;

        result.hit = result.damage != 0 || !missable;

        return result;        
    }
}
