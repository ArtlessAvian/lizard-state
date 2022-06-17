using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class ReachAttackAction : Action
{
    ReachAttackData data;
    public ReachAttackAction(ReachAttackData data)
    {
        this.data = data ?? ResourceLoader.Load<ReachAttackData>("res://Crawler/Model/Attacks/ReachAttacks/VeryPositive.tres");
    }

    public override bool Do(Model model, Entity e)
    {
        if (data is null) { data = ResourceLoader.Load<ReachAttackData>("res://Crawler/Model/Attacks/ReachAttacks/Poke.tres"); }
        GD.Print(data.ResourceName);

        (int x, int y) targetPos = GetTargetPos(e.position);
        if (data.startup > 0)
        {
            model.CoolerApiEvent(e.id, "AttackStartup", new Vector2(targetPos.x, targetPos.y));
        }

        e.nextMove += data.startup;
        e.queuedAction = new ReachAttackActive(data).SetTarget(targetPos);

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        // TODO
        // if (e.energy < data.energy)
        // {
        //     return false;
        // }

        return true;
    }

    public override (int, int) Range => (1, data.range);

    public class ReachAttackActive : Action
    {
        private ReachAttackData data;

        // not sure if this is the right access modifier.
        internal ReachAttackActive(ReachAttackData data)
        {
            this.data = data;
        }

        public ReachAttackActive(Godot.Collections.Dictionary dict) : base(dict)
        {

        }

        public override bool Do(Model model, Entity e)
        {
            (int x, int y) targetPos = GetTargetPos(e.position);
            Entity targeted = model.GetEntityAt(targetPos);

            // e.energy -= data.energy;

            model.CoolerApiEvent(-1, "Wait");
            model.CoolerApiEvent(e.id, "AttackActive", new Vector2(targetPos.x, targetPos.y));

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
                    GD.Print(data.knockback);
                    targeted.position = knockback;
                    model.CoolerApiEvent(targeted.id, "Knockback", new Vector2(knockback.x, knockback.y));
                }
            }
            else
            {
                // whiff!
            }

            e.nextMove += data.recovery;

            model.CoolerApiEvent(-1, "Wait");
            return true;
        }

        public override bool IsValid(Model model, Entity e)
        {
            return true;
        }

        private void OnHit(Model model, Entity e, Entity targeted)
        {
            targeted.health -= data.damage;

            // think of it as "lose {stun} turns." The term here (VVVVVVVVV) ensures that lower id's lose their turn.
            int stunUntil = model.time + data.stun + (targeted.id < e.id ? 1 : 0);
            targeted.nextMove = Math.Max(targeted.nextMove, stunUntil);
            targeted.stunned = true;

            if (targeted.health <= 0)
            {
                targeted.downed = true;
                targeted.nextMove = -1;
            }

            model.CoolerApiEvent(new Dictionary(){
                    {"subject", e.id},
                    {"action", "Hit"},
                    {"object", targeted.id},
                    {"damage", data.damage}
                });

            if (targeted.health <= 0)
            {
                model.CoolerApiEvent(targeted.id, "Downed");
            }
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

                if (model.Map.TileIsWall(position))
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
}