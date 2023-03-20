using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

// TODO: somehow limit visibility to actions?
public class ActionUtils
{
    // If calling on multiple entities, start with the outer ones first.
    // BUG: This can put people inside walls. Or, inside each other.
    // TODO: If they hit a wall, they should "wallsplat" or something.
    public static void ApplyKnockback(Model model, Entity e, Entity targeted, int knockback)
    {
        (int, int) destination = GridHelper.StepThrough(e.position, targeted.position, knockback + GridHelper.Distance(e.position, targeted.position));

        (int x, int y) safePos = targeted.position; // the enemy's current position is always valid.
        foreach ((int, int) tentativePos in GridHelper.LineBetween(targeted.position, destination))
        {
            if (tentativePos == targeted.position) { continue; }

            if (!model.CanWalkFromTo(safePos, tentativePos))
            {
                break;
            }
            // TODO: if theres an entity at position, knock them down.
            if (model.GetEntityAt(tentativePos) is Entity occupier)
            {
                occupier.KnockdownForTurns(1, model.time, e.id);

                model.CoolerApiEvent(new Dictionary(){
                    {"subject", e.id},
                    {"action", "Hit"},
                    {"object", occupier.id},
                    {"damage", 0},
                    {"swept", true},
                });
            }

            safePos = tentativePos;
        }
        targeted.position = safePos;
        model.CoolerApiEvent(e.id, "Knockback", new Vector2(safePos.x, safePos.y), targeted.id);
    }
}