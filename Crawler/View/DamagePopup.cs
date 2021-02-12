using Godot;
using System;

public class DamagePopup : Label
{
    // TODO: Make cool. Keeping it simple for now.
    // private static Vector2 offset = new Vector2(-40, 0); 
    
    private float timer;
    private Vector2 velocity;

    public override void _Ready()
    {
        velocity = Vector2.Up * 20;
        velocity += Vector2.Right * 10 * (GD.Randf() * 2 - 1);
    }

    public override void _Process(float delta)
    {
        velocity += Vector2.Down * delta * 40;
        this.RectPosition = this.RectPosition + velocity * delta;

        timer += delta;

        if (timer > 1)
        {
            QueueFree();
        }
    }
}
