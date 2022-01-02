using System;
using Godot;
using Godot.Collections;

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

    private class ReachAttackActive : Action
    {
        private ReachAttackData data;

        public ReachAttackActive(ReachAttackData data)
        {
            this.data = data;
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
    }
}