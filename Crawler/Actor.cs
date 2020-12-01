using Godot;
using System;

// Like a ViewModel. Also, a pile of callbacks for the View.
public partial class Actor : Node2D
{
    (int x, int y) targetPosition;

    public void SyncWithEntity(Entity subject)
    {
        targetPosition = subject.position;
        Position = new Vector2(
            targetPosition.x * Crawler.TILESIZE.x,
            targetPosition.y * Crawler.TILESIZE.y
        );
    }

    public void Perform(string action, string args)
    {
        EmitSignal("Action", action);
        EmitSignal(action, args);

        if (action == "Moved")
        {
            string[] tokenized = args.Split(",");
            targetPosition.x = int.Parse(tokenized[0]);
            targetPosition.y = int.Parse(tokenized[1]);
        }
    }

    public override void _Process(float delta)
    {
        Position = Position.LinearInterpolate(
            new Vector2(
                targetPosition.x * Crawler.TILESIZE.x,
                targetPosition.y * Crawler.TILESIZE.y
            ),
            1 - Mathf.Pow(1-0.3f, delta * 60f)
        );
    }
}
