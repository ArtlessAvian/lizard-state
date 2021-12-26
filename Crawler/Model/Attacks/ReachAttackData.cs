using Godot;

public abstract class ReachAttackData : Resource
{
    [Export] public int startup = 5;
    [Export] public int recovery = 5;
    [Export] public int stun = 10;
    [Export] public int damage = 2;
    [Export] public float range = 2;

    public void DoAttack(Entity target, int timeNow)
    {

        // return result;        
    }
}
