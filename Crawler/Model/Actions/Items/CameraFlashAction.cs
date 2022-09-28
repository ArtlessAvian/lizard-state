using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;


public class CameraFlashAction : Action
{
    public override Dictionary Do(Model model, Entity e)
    {
        (int x, int y) targetPos = GetTargetPos(e.position);

        // model.CoolerApiEvent(e.id, "AttackActive", new Vector2(e.position.x, e.position.y));
        Array<Dictionary> results = new Array<Dictionary>();
        Dictionary modelEvent = new Dictionary(){
                {"subject", e.id},
                {"action", "AttackActive"},
                {"args", new Vector2(e.position.x, e.position.y)},
                {"flavorTags", new List<string>{"Flash", "Shoot"}},
                {"results", results},
            };

        HashSet<(int, int)> set = new HashSet<(int, int)>(VisibilityTrie.ConeOfView(((int x, int y) pos) => false, 5, (targetPos.x - e.position.x, targetPos.y - e.position.y), 45));
        foreach ((int dx, int dy) in set)
        {
            if (dx == 0 && dy == 0) { continue; }

            if (model.GetEntityAt((e.position.x + dx, e.position.y + dy)) is Entity targeted)
            {
                results.Add(OnHit(model, e, targeted));
            }
        }

        e.nextMove += 1;

        return modelEvent;
    }

    private Dictionary OnHit(Model model, Entity e, Entity targeted)
    {
        targeted.StunForTurns(1, model.time, e.id);

        return new Dictionary(){
                {"subject", e.id},
                {"action", "Hit"},
                {"object", targeted.id},
                {"damage", 0},
                {"flavorTags", new Array<string>(){"Flash", "Shoot"}}
            };
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }

    public override (int min, int max) Range => (1, 5);
    public override TargetingType.Type TargetingType => new TargetingType.Cone(45);
}