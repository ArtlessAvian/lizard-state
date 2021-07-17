using Godot;
using System;

public struct AttackResult
{
    public bool stuns;
    public int damage;
    public int stunUntil;
}

public class AttackData : Resource
{
    [Export] public int range = 1;
    [Export] public int energy = 0;
    [Export] public int recovery = 10;

    [Export] public float comboStartChance;
    [Export] public float comboLinkChance;
    [Export] public int hitDamage;
    [Export] public int missDamage;
    [Export] public int stun;

    public AttackResult TryAttack(Entity target, int timeNow)
    {
        AttackResult result;
        result.stuns = GD.Randf() < (target.stunned ? comboLinkChance : comboStartChance);
        result.damage = result.stuns ? hitDamage : missDamage;
        result.stunUntil = result.stuns ? timeNow + stun : 0;
        return result;
    }
}
