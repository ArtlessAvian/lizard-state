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

    public void Perform(string action, object args)
    {
        // EmitSignal("Action", action);
        // EmitSignal(action, args);

        if (action == "Moved")
        {
            (int x, int y) cast = ((int x, int y))args;
            targetPosition.x = cast.x;
            targetPosition.y = cast.y;
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
