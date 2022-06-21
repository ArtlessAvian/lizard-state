// using Godot;

// public abstract class RushAttackData : Resource
// {
//     [Export] public int recovery = 10; // no recovery on hit.
//     [Export] public int damagePerHit = 1;
//     [Export] public float expectedDamage = 3;

//     public float MissChance
//     {
//         // E[Geom(p) * n] = n (1-p)/p
//         // set { expectedDamage = damagePerHit * (1-value)/value; }
//         get { return damagePerHit / (expectedDamage + damagePerHit); }
//     }

//     public float Variance
//     {
//         get { return expectedDamage / MissChance; }
//     }

//     public void DoAttack(Entity target, int timeNow)
//     {

//         // return result;        
//     }
// }
