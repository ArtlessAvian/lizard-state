using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;


public class FlashbangAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        model.CoolerApiEvent(-1, "Wait");
        model.CoolerApiEvent(e.id, "AttackActive", new Vector2(e.position.x, e.position.y));
        model.CoolerApiEvent(e.id, "CameraFlash");

        HashSet<(int, int)> set = new HashSet<(int, int)>(VisibilityTrie.FieldOfView(((int x, int y) pos) => false, 1));
        foreach ((int dx, int dy) in set)
        {
            if (model.GetEntityAt((targetPos.x + dx, targetPos.y + dy)) is Entity targeted)
            {
                // think of it as "lose {stun} turns." (VVVVVVVVV) The term here ensures that lower id's lose their turn.
                int stunUntil = model.time + 2 + (targeted.id < e.id ? 1 : 0);
                targeted.nextMove = Math.Max(targeted.nextMove, stunUntil);
                targeted.stunned = true;

                model.CoolerApiEvent(new Dictionary(){
                    {"subject", e.id},
                    {"action", "Hit"},
                    {"object", targeted.id},
                    {"damage", 0}
                });
            }
        }

        e.nextMove += 1;

        model.CoolerApiEvent(-1, "Wait");

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);
        if (GridHelper.Distance(e.position, GetTargetPos(e.position)) > Range.max) { return false; }
        return true;
    }

    public override (int min, int max) Range => (1, 5);
    public override TargetingType.Type targetingType => new TargetingType.Smite(1);
}