using Godot;
using Godot.Collections;

public class EnderAttackData : AttackData
{
    [Export] public float comboStartChance;
    [Export] public float comboLinkChance;
    [Export] public int hitDamage;
    [Export] public int stun;

    public override AttackResult DoAttack(Entity target, int timeNow)
    {
        AttackResult result;
        result.hit = GD.Randf() < (target.stunned ? comboLinkChance : comboStartChance);
        result.stuns = result.hit;
        result.damage = result.stuns ? hitDamage : 0;
        result.stunUntil = result.stuns ? timeNow + stun : 0;
        
        return result;
    }
}