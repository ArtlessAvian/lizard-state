using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;


public class FlashbangAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);

        model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "AttackActive"},
                {"args", new Vector2(targetPos.x, targetPos.y)},
                {"flavorTags", new Godot.Collections.Array(){"Flash", "Shoot"}}
            });

        HashSet<AbsolutePosition> set = new HashSet<AbsolutePosition>(VisibilityTrie.FieldOfView(targetPos, pos => false, 1));
        foreach (AbsolutePosition splashedPos in set)
        {
            if (model.GetEntityAt(splashedPos) is Entity targeted)
            {
                // think of it as "lose {stun} turns." (VVVVVVVVV) The term here ensures that lower id's lose their turn.
                targeted.StunForTurns(2, model.time, e.id);

                model.CoolerApiEvent(new Dictionary(){
                    {"subject", e.id},
                    {"action", "Hit"},
                    {"object", targeted.id},
                    {"damage", 0},
                    {"flavorTags", new Godot.Collections.Array(){"Flash", "Shoot"}}
                });
            }
        }

        e.nextMove += 1;

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        AbsolutePosition targetPos = GetTargetPos(e.position);
        if (GridHelper.Distance(e.position, GetTargetPos(e.position)) > Range.max) { return false; }
        return true;
    }

    public override (int min, int max) Range => (1, 5);
    public override TargetingType.Type TargetingType => new TargetingType.Smite(1);
}