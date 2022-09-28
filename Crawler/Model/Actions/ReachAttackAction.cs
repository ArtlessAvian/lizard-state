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

    [Export] public List<string> flavorTags = new List<string>();

    private ReachAttackAction() { }

    public override Dictionary Do(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        e.nextMove += startup;
        CSharpScript followup = GD.Load("res://Crawler/Model/Actions/ReachAttackFollowup.cs") as CSharpScript;
        e.queuedAction = followup.New(this) as Action;
        e.queuedAction = e.queuedAction.SetTarget(targetPos);

        return CreateModelEvent(e.id, "AttackStartup", new Vector2(targetPos.x, targetPos.y));
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

    public override (int, int) Range => (1, range);
}