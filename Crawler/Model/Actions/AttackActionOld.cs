// using Godot;
// using System.Collections.Generic;

// public struct AttackResultOld
// {
//     public bool hit;
//     public int damage;
//     public int stunUntil;

//     public AttackResultOld(Entity target, Attack attack, Entity attacker)
//     {
//         hit = GD.Randf() < (target.stunned ? attack.comboLinkChance : attack.comboStartChance);
//         damage = hit ? attack.hitDamage : attack.missDamage;
//         stunUntil = attacker.nextMove + attack.stun;
//     }
// }

// public struct AttackOld
// {
//     public float comboStartChance;
//     public float comboLinkChance;
//     public int hitDamage;
//     public int missDamage;
//     public int stun;

//     public AttackOld(float comboStartChance, float comboLinkChance, int hitDamage, int missDamage, int stun)
//     {
//         this.comboStartChance = comboStartChance;
//         this.comboLinkChance = comboLinkChance;
//         this.hitDamage = hitDamage;
//         this.missDamage = missDamage;
//         this.stun = stun;
//     }
// }

// public class AttackActionOld : Action
// {
//     static AttackOld attack = new AttackOld(0.3f, 0.2f, 3, 1, 5);
//     (int x, int y) direction;

//     public AttackActionOld(object args) 
//     {
//         direction = ((int, int))args;
//     }

//     public bool Do(ModelAPI api, Entity e)
//     {
//         Entity target = api.GetEntityAt(e.position.x + direction.x, e.position.y + direction.y);

//         if ((target is null) || target == e)
//         {
//             return false;
//         }

//         // All logic here
//         api.NewEvent(new ModelEvent(-1, "Wait"));
        
//         BeforeAttack(api, e);
//         RollForAttack(api, e, target);

//         api.NewEvent(new ModelEvent(-1, "Wait"));

//         return true;
//     }

//     private void BeforeAttack(ModelAPI api, Entity e)
//     {
//         e.nextMove += 10;
//     }

//     private void RollForAttack(ModelAPI api, Entity e, Entity target)
//     {
//         AttackResult roll = new AttackResult(target, attack, e);
//         target.TakeDamage(roll);

//         api.NewEvent(new ModelEvent(e.id, "Attack", roll, target.id));
//         api.NewEvent(new ModelEvent(-1, "Print", $"{e.species.displayName} hits {target.species.displayName}!"));
//         if (target.downed)
//             api.NewEvent(new ModelEvent(-1, "Print", $"{target.species.displayName} is downed!!"));
//         else if (roll.hit)
//             api.NewEvent(new ModelEvent(-1, "Print", $"{target.species.displayName} stumbles!!"));
//     }

//     private void AfterAttack(ModelAPI api, Entity e) {}
// }
