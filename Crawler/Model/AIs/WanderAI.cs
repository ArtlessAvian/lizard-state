using System.Collections.Generic;
using Godot;

public class WanderAI : AI
{
    public override IEnumerable<(Action, bool)> GetMoves(Model model, Entity e)
    {
        // TODO: Actually make interesting.
        for (int i = 0; i < 10; i++)
        {
            Action action = new MoveAction().SetTargetRelative(((int)(GD.Randi() % 3) - 1, (int)(GD.Randi() % 3) - 1));
            yield return (action, false);
        }
        yield return (new MoveAction().SetTargetRelative((0, 0)), true);
    }
}