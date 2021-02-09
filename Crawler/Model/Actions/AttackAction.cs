using Godot;
using System.Collections.Generic;

public struct AttackResult
{
    public bool hit;
    public int damage;
    public int stunUntil;

    public AttackResult(Entity target, Attack attack, Entity attacker)
    {
        hit = GD.Randf() < (target.stunned ? attack.comboLinkChance : attack.comboStartChance);
        damage = hit ? attack.hitDamage : attack.missDamage;
        stunUntil = attacker.nextMove + attack.stun;
    }
}

public struct Attack
{
    public float comboStartChance;
    public float comboLinkChance;
    public int hitDamage;
    public int missDamage;
    public int stun;

    public Attack(float comboStartChance, float comboLinkChance, int hitDamage, int missDamage, int stun)
    {
        this.comboStartChance = comboStartChance;
        this.comboLinkChance = comboLinkChance;
        this.hitDamage = hitDamage;
        this.missDamage = missDamage;
        this.stun = stun;
    }
}

public class AttackAction : Action
{
    static Attack attack = new Attack(0.3f, 0.2f, 3, 1, 5);
    (int x, int y) direction;

    public AttackAction(object args) 
    {
        direction = ((int, int))args;
    }

    public bool Do(ModelAPI api, Entity e)
    {
        Entity target = api.GetEntityAt(e.position.x + direction.x, e.position.y + direction.y);

        if ((target is null) || target == e)
        {
            return false;
        }

        // All logic here
        e.nextMove += 10;

        AttackResult roll = new AttackResult(target, attack, e);
        target.TakeDamage(roll);

        // Consider moving to actors?
        api.NewEvent(new ModelEvent(-1, "Wait"));

        api.NewEvent(new ModelEvent(e.id, "Attack", roll, target.id));

        api.NewEvent(new ModelEvent(-1, "Print", $"{e.species.displayName} hits {target.species.displayName}!"));
        if (target.downed)
            api.NewEvent(new ModelEvent(-1, "Print", $"{target.species.displayName} is downed!!"));
        else if (roll.hit)
            api.NewEvent(new ModelEvent(-1, "Print", $"{target.species.displayName} stumbles!!"));
                
        api.NewEvent(new ModelEvent(-1, "Wait"));

        return true;
    }
}
