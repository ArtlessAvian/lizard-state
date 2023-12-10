using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using LizardState.Engine;

public class DelayAction : CrawlAction
{
    [Export] public int startup = 0;
    [Export] public CrawlAction actionPrototype = null;

    private CrawlAction actionInstanceNullable;
    private CrawlAction actionInstance
    {
        get
        {
            actionInstanceNullable = actionInstanceNullable ?? actionPrototype.Duplicate() as CrawlAction;
            return actionInstanceNullable;
        }
    }

    public override bool Do(Model model, Entity e)
    {
        if (!IsValid(model, e)) { return false; }

        actionInstance.SetTarget(GetTargetPos(e.position));
        e.queuedAction = actionInstance;

        e.nextMove += startup;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        actionInstance.SetTarget(GetTargetPos(e.position));
        return actionInstance.IsValid(model, e);
    }

    public override (int min, int max) Range => actionInstance.Range;
}