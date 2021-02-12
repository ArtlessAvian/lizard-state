using Godot;
using System;
using System.Collections.Generic;

// Like a ViewModel. Also, a pile of callbacks for the View.
public partial class Actor : Node2D
{
    (int x, int y) targetPosition;
    int health = 0;
    bool stunned = false;

    public void SyncWithEntity(Entity subject)
    {
        targetPosition = subject.position;
        Position = new Vector2(
            targetPosition.x * Crawler.TILESIZE.x,
            targetPosition.y * Crawler.TILESIZE.y
        );

        health = subject.health;
        TextureProgress healthbar = GetNode<TextureProgress>("HealthBar");
        healthbar.MaxValue = subject.species.maxHealth;
        healthbar.Value = subject.health;

        stunned = subject.stunned;
        AnimatedSprite aniSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        aniSprite.Frame = subject.stunned ? 1 : 0;
    }

    private void FaceDirection(int dx, int dy)
    {
        if (dy == 0 && dx == 0) { return; }

        AnimatedSprite sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        if (dx != 0)
        {
            sprite.Animation = "East";
            sprite.FlipH = dx < 0;
        }
        else
        {
            sprite.Animation = dy > 0 ? "South" : "North";
        }
    }

    public bool IsAnimating()
    {
        if (Math.Abs(targetPosition.x - Position.x / Crawler.TILESIZE.x) > 0.01) { return true; }
        if (Math.Abs(targetPosition.y - Position.y / Crawler.TILESIZE.y) > 0.01) { return true; }
        if (GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying()) { return true; }
        // if (GetNode<AnimationPlayer>("AnimationPlayer").) { return true; }
        return false;
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
