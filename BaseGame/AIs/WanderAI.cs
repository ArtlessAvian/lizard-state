using System;
using System.Collections.Generic;
using Godot;
using LizardState.Engine;

[Obsolete]
public class WanderAI : AI
{
    public override IEnumerable<(CrawlAction, bool)> GetMoves(Model model, Entity e)
    {
        // TODO: Actually make interesting.
        for (int i = 0; i < 10; i++)
        {
            CrawlAction action = new MoveAction().SetTargetRelative(new Vector2i((int)(GD.Randi() % 3) - 1, (int)(GD.Randi() % 3) - 1));
            yield return (action, false);
        }
        yield return (new MoveAction().SetTargetRelative(Vector2i.ZERO), true);
    }
}