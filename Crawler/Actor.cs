using Godot;
using System;

// Like a ViewModel. Also, a pile of callbacks for the View.
public class Actor : Node2D
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

    // A pile of callbacks.
    [Signal]
    public delegate void Action(String action);
    [Signal]
    public delegate void Created(String args);
    [Signal]
    public delegate void Damaged(String args);

    public void Perform(string action, string args)
    {
        EmitSignal("Action", action);
        EmitSignal(action, args);
    }
}
