using Godot;
using Godot.Collections;

public class PokeAttackData : AttackData
{
    [Export] public float comboStartChance;
    [Export] public int hitDamage;
    [Export] public int chipDamage;
    [Export] public int stun;

    public override AttackResult DoAttack(Entity target, int timeNow)
    {
        AttackResult result;
        result.hit = true;
        result.stuns = GD.Randf() < comboStartChance;
        result.damage = result.stuns ? hitDamage : chipDamage;
        result.stunUntil = result.stuns ? timeNow + stun : 0;

        


        return result;
    }
}