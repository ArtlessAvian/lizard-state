using Godot;
using Godot.Collections;

public class RushAttackAction : Action
{
    [Export] public int damagePerHit = 1;
    [Export] public float expectedDamage = 3;

    public float MissChance
    {
        // E[Geom(p) * n] = n * 1/p. solve for p
        get { return damagePerHit / expectedDamage; }
        // E[Geom(p) * n] = n (1-p)/p // if first hit not guaranteed
    }

    public float Variance
    {
        get { return expectedDamage / MissChance; }
    }

    private RushAttackAction() { }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return false;
        }

        (int x, int y) targetPos = GetTargetPos(e.position);
        Entity targeted = model.GetEntityAt(targetPos);

        do
        {
            // TODO: Move into entity?

            // model.Debug($"{targeted.ResourceName}: ouch");
            targeted.health -= damagePerHit;
            targeted.queuedAction = null;

            if (targeted.health <= 0)
            {
                targeted.downed = true;
                targeted.nextMove = -1;
            }

            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Rush"},
                {"object", targeted.id},
                {"damage", damagePerHit}
            });

            if (targeted.health <= 0)
            {
                model.CoolerApiEvent(targeted.id, "Downed");
            }
        } while (GD.Randf() >= MissChance && !targeted.downed);

        e.nextMove += 1;

        if (!targeted.downed)
        {
            model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Miss"},
                {"object", targeted.id}
            });
        }

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);
        // TODO: Add raycast to target.

        if (GridHelper.Distance(e.position, targetPos) > 1.5f)
        {
            return false;
        }

        Entity targeted = model.GetEntityAt(targetPos);
        if ((targeted is null) || targeted == e)
        {
            return false;
        }

        return true;
    }
}
