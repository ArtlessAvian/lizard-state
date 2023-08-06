using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using LizardState.Engine;

public class ReachAttackFollowup : CrawlAction
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
        AbsolutePosition targetPos = GetTargetPos(e.position);
        model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "AttackActive"},
                {"args", new Vector2(targetPos.x, targetPos.y)},
                {"flavorTags", data.flavorTags}
            });

        if (GetHitEntity(model, e, targetPos) is Entity targeted)
        {
            if (GD.Randf() < data.blockChance)
            {
                // block!
                model.Debug($"{e.species.displayName} missed!");
            }
            else
            {
                targeted.queuedAction = null;
                // clean hit!
                OnHit(model, e, targeted);

                ActionUtils.ApplyKnockback(model, e, targeted, data.knockback);
            }
        }
        else
        {
            model.CoolerApiEvent(e.id, "Whiff");
        }

        e.nextMove += data.recovery;

        return true;
    }

    private Entity GetHitEntity(Model model, Entity e, AbsolutePosition targetPos)
    {
        if (data.smiteTargeting)
        {
            return model.GetEntityAt(targetPos);
        }

        foreach (AbsolutePosition hitPos in GridHelper.LineBetween(e.position, targetPos))
        {
            if (model.GetEntityAt(hitPos) is Entity targeted && targeted != e)
            {
                return targeted;
            }
        }
        return null;
    }

    public override bool IsValid(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);
        if (GridHelper.Distance(e.position, GetTargetPos(e.position)) > Range.max) { return false; }
        if (GridHelper.Distance(e.position, GetTargetPos(e.position)) < Range.min) { return false; }
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
}