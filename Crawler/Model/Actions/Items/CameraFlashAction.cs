using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;


public class CameraFlashAction : Action
{
    [Export]
    private ReachAttackData data;

    public override bool Do(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        model.CoolerApiEvent(-1, "Wait");
        model.CoolerApiEvent(e.id, "AttackActive", new Vector2(targetPos.x, targetPos.y));
        model.CoolerApiEvent(e.id, "CameraFlash");

        foreach ((int dx, int dy) in VisibilityTrie.ConeOfView(((int x, int y) pos) => true, 5, (targetPos.x - e.position.x, targetPos.y - e.position.y), 45))
        {
            if (model.GetEntityAt((targetPos.x + dx, targetPos.y + dy)) is Entity targeted)
            {
                // think of it as "lose {stun} turns." (VVVVVVVVV) The term here ensures that lower id's lose their turn.
                int stunUntil = model.time + 1 + (targeted.id < e.id ? 1 : 0);
                targeted.nextMove = Math.Max(targeted.nextMove, stunUntil);
                targeted.stunned = true;
            }
        }

        e.nextMove += 1;

        model.CoolerApiEvent(-1, "Wait");

        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }
}