using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;


public class CameraFlashAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        model.CoolerApiEvent(e.id, "AttackActive", new Vector2(e.position.x, e.position.y));
        model.CoolerApiEvent(e.id, "CameraFlash");

        HashSet<(int, int)> set = new HashSet<(int, int)>(VisibilityTrie.ConeOfView(((int x, int y) pos) => false, 5, (targetPos.x - e.position.x, targetPos.y - e.position.y), 45));
        foreach ((int dx, int dy) in set)
        {
            if (dx == 0 && dy == 0) { continue; }

            if (model.GetEntityAt((e.position.x + dx, e.position.y + dy)) is Entity targeted)
            {
                // think of it as "lose {stun} turns." (VVVVVVVVV) The term here ensures that lower id's lose their turn.
                int stunUntil = model.time + 1 + (targeted.id < e.id ? 1 : 0);
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

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }

    public override (int min, int max) Range => (1, 5);
    public override TargetingType.Type TargetingType => new TargetingType.Cone(45);
}