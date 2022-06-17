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
        CSharpScript followup = GD.Load("res://Crawler/Model/Actions/ReachAttackFollowup.cs") as CSharpScript;
        e.queuedAction = followup.New(data) as Action;
        e.queuedAction = e.queuedAction.SetTarget(targetPos);

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
}