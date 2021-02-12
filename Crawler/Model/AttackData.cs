using Godot;
using System;

public struct AttackResult
{
    public bool hit;
    public int damage;
    public int stunUntil;
}

public class AttackData : Resource
{
    [Export] public int range = 1;

    [Export] public float comboStartChance;
    [Export] public float comboLinkChance;
    [Export] public int hitDamage;
    [Export] public int missDamage;
    [Export] public int stun;

    public AttackResult TryAttack(Entity target, int timeNow)
    {
        AttackResult result;
        result.hit = GD.Randf() < (target.stunned ? comboLinkChance : comboStartChance);
        result.damage = result.hit ? hitDamage : missDamage;
        result.stunUntil = result.hit ? timeNow + stun : 0;
        return result;
    }
}
