using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class ReachAttackAction : Action
{
    [Export] public int startup = 0;
    [Export] public int recovery = 3;
    [Export] public int stun = 1;
    [Export] public int damage = 2;
    [Export] public float blockChance = 0.2f;
    [Export] public int range = 7;
    [Export] public int knockback = 0;
    [Export] public bool sweeps = false;
    [Export] public bool smiteTargeting = false;

    [Export] public List<string> flavorTags = new List<string>();

    private ReachAttackAction() { }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e)) { return false; }

        AbsolutePosition targetPos = GetTargetPos(e.position);
        if (startup > 0)
        {
            model.CoolerApiEvent(e.id, "AttackStartup", new Vector2(targetPos.x, targetPos.y));
        }

        e.nextMove += startup;
        CSharpScript followup = GD.Load("res://Crawler/Model/Actions/ReachAttackFollowup.cs") as CSharpScript;
        e.queuedAction = followup.New(this) as Action;
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
        AbsolutePosition targetPos = GetTargetPos(e.position);
        GD.Print(GridHelper.Distance(e.position, GetTargetPos(e.position)), range);
        if (GridHelper.Distance(e.position, GetTargetPos(e.position)) > Range.max) { return false; }
        if (GridHelper.Distance(e.position, GetTargetPos(e.position)) < Range.min) { return false; }
        return true;
    }

    public override (int min, int max) Range => (1, range);
}