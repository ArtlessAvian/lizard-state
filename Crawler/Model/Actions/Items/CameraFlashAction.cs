using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;


public class CameraFlashAction : Action
{
    public override bool Do(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        // model.CoolerApiEvent(e.id, "AttackActive", new Vector2(e.position.x, e.position.y));
        model.CoolerApiEvent(new Dictionary(){
                {"subject", e.id},
                {"action", "AttackActive"},
                {"args", new Vector2(e.position.x, e.position.y)},
                {"flavorTags", new List<string>{"Flash", "Shoot"}}
            });
        model.CoolerApiEvent(e.id, "CameraFlash");

        HashSet<AbsolutePosition> set = new HashSet<AbsolutePosition>(VisibilityTrie.ConeOfView(e.position, pos => false, 5, (targetPos.x - e.position.x, targetPos.y - e.position.y), 45));
        foreach ((int x, int y) tile in set)
        {
            if (tile.x == e.position.x && tile.y == e.position.y) { continue; }

            if (model.GetEntityAt((tile.x, tile.y)) is Entity targeted)
            {
                targeted.StunForTurns(1, model.time, e.id);

                model.CoolerApiEvent(new Dictionary(){
                    {"subject", e.id},
                    {"action", "Hit"},
                    {"object", targeted.id},
                    {"damage", 0},
                    {"flavorTags", new List<string>(){"Flash", "Shoot"}}
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