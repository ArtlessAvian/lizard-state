
using Godot;

public class WanderAI : AI
{
    public override Action GetMove(Model model, Entity e)
    {
        // TODO: Actually make interesting.
        Action action = new MoveAction().SetTargetRelative(((int)(GD.Randi() % 3) - 1, (int)(GD.Randi() % 3) - 1));
        for (int i = 0; i < 10; i++)
        {
            if (action.IsValid(model, e)) {return action;}
            action = new MoveAction().SetTargetRelative(((int)(GD.Randi() % 3) - 1, (int)(GD.Randi() % 3) - 1));
        }
        return action;
    }
}