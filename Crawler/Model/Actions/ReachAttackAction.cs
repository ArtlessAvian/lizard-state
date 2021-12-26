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
        model.CoolerApiEvent(e.id, "FaceDirection", new Vector2(targetPos.x, targetPos.y));
        model.CoolerApiEvent(e.id, "AttackStartup");

        e.nextMove += data.startup;
        e.queuedAction = new ReachAttackActive(data).SetTarget(targetPos);

        model.Debug($"ready {model.time}");
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

    public override (float, float) Range => (1, data.range);

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
            model.CoolerApiEvent(e.id, "AttackActive");

            if (targeted is object)
            {
                targeted.health -= data.damage;
                targeted.nextMove = Math.Max(targeted.nextMove, model.time + data.stun);
                targeted.stunned = true;
                targeted.queuedAction = null;
                
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

            e.nextMove += data.recovery;

            model.CoolerApiEvent(-1, "Wait");
            return true;
        }

        public override bool IsValid(Model model, Entity e)
        {
            return true;
        }
    }
}