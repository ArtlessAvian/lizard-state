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

    public abstract AttackResult DoAttack(Entity target, int timeNow);

    public virtual Action CreateAction()
    {
        return new AttackAction(this);
    }
}
