using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;


public class ReachAttackFollowup : Action
{
    [Export]
    private ReachAttackAction data;

    // not sure if this is the right access modifier.
    public ReachAttackFollowup(ReachAttackAction data)
    {
        this.data = data;
    }

    private ReachAttackFollowup() { }

    public override bool Do(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);
        Entity targeted = model.GetEntityAt(targetPos);

        // e.energy -= data.energy;

        model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "AttackActive"},
                {"args", new Vector2(targetPos.x, targetPos.y)},
                {"flavorTags", data.flavorTags}
            });

        if (targeted is object)
        {
            targeted.queuedAction = null;

            if (GD.Randf() < data.blockChance)
            {
                // block!
                model.Debug($"{e.species.displayName} missed!");
            }
            else
            {
                // clean hit!
                OnHit(model, e, targeted);

                // TODO: This can put people inside walls. Or, inside each other.
                // (If they intersect a wall, they should "wallsplat" or something.)
                // (If they end up on a person, they should pop to a random nearby tile.)
                (int x, int y) knockback = KnockbackPosition(model, e.position, targeted.position, data.knockback);
                targeted.position = knockback;
                model.CoolerApiEvent(e.id, "Knockback", new Vector2(knockback.x, knockback.y), targeted.id);
            }
        }
        else
        {
            model.CoolerApiEvent(e.id, "Whiff");
        }

        e.nextMove += data.recovery;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);
        if (GridHelper.Distance(e.position, GetTargetPos(e.position)) > Range.max) { return false; }
        return true;
    }

    private void OnHit(Model model, Entity e, Entity targeted)
    {
        targeted.health -= data.damage;

        if (data.sweeps)
        {
            targeted.KnockdownForTurns(data.stun, model.time, e.id);
        }
        else
        {
            targeted.StunForTurns(data.stun, model.time, e.id);
        }

        model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "Hit"},
                {"object", targeted.id},
                {"damage", data.damage},
                {"swept", data.sweeps},
                {"flavorTags", data.flavorTags},
            });
    }

    private (int, int) KnockbackPosition(Model model, (int x, int y) from, (int x, int y) to, int howMuch)
    {
        // reflect "from" around "to", then ray from "to" to "reflected."
        (int x, int y) reflected = ((to.x - from.x) + to.x, (to.y - from.y) + to.y);
        IEnumerable<(int x, int y)> enumerable = GridHelper.RayThrough(to, reflected);

        // haha yeaa
        // BUG: I was able to punch someone into a corner. Very weird. 
        (int, int) previousPosition = to; // the enemy's current position is always valid.
        foreach ((int, int) position in enumerable)
        {
            if (howMuch <= 0)
            {
                return position;
            }
            howMuch--;

            if (model.map.TileIsWall(position))
            {
                return previousPosition;
            }

            previousPosition = position;
        }

        return (0, 0); // this will never happen
    }

    // private (int, int) KnockbackPosition((int x, int y) from, (int x, int y) to, int howMuch)
    // {
    //     (int dx, int dy, int octant) = GridHelper.Octantify(to.x - from.x, to.y - from.y);

    //     dy = dy + (int)(dy * howMuch / (float)dx);
    //     dx = dx + howMuch;

    //     (dx, dy) = GridHelper.DeOctantify(dx, dy, octant);

    //     return (from.x + dx, from.y + dy);
    // }
}