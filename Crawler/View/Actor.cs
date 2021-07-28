using Godot;
using System;
using System.Collections.Generic;

// Like a ViewModel. Also, a pile of callbacks for the View.
public partial class Actor : Node2D
{
    public Entity role;

    // vector2i
    public Vector2 targetPosition;
    int health = 0;
    bool stunned = false;
    bool seen = false;

    // TODO: Temporary
    public string displayName;

    public void ActAs(Entity role)
    {
        this.role = role;
        ModelSync();
    }

    public void ModelSync()
    {
        targetPosition = new Vector2((int)role.position.x, (int)role.position.y);
        // Position = new Vector2(
        //     targetPosition.x * View.TILESIZE.x,
        //     targetPosition.y * View.TILESIZE.y
        // );
        Position = targetPosition * View.TILESIZE; // elementwise

        health = role.health;
        TextureProgress healthbar = GetNode<TextureProgress>("HealthBar");
        healthbar.MaxValue = role.species.maxHealth;
        healthbar.Value = role.health;

        if (health <= 0) { this.Visible = false; }

        stunned = role.stunned;
        AnimatedSprite aniSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        // aniSprite.Frames = GD.Load<SpriteFrames>($"res://Crawler/View/ActorData/{role.species.ResourceName}.tres");
        aniSprite.Frame = role.stunned ? 1 : 0;

        // TODO: Temporary
        displayName = role.species.displayName;
    }

    private void FaceDirection(float dx, float dy)
    {
        if (dy == 0 && dx == 0) { return; }

        AnimatedSprite sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        int frame = sprite.Frame;
        if (dx != 0)
        {
            sprite.Animation = "East";
            sprite.FlipH = dx < 0;
        }
        else
        {
            sprite.Animation = dy > 0 ? "South" : "North";
        }
        sprite.Frame = frame;
    }

    private void FacePosition((int x, int y) position)
    {
        FaceDirection(position.x - targetPosition.x, position.y - targetPosition.y);
    }

    private void FacePosition(Vector2 position)
    {
        FacePosition(((int x, int y))(position.x, position.y));
    }

    public bool IsAnimating()
    {
        if (Math.Abs(targetPosition.x - Position.x / View.TILESIZE.x) > 0.01) { return true; }
        if (Math.Abs(targetPosition.y - Position.y / View.TILESIZE.y) > 0.01) { return true; }
        if (GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying()) { return true; }
        // if (GetNode<AnimationPlayer>("AnimationPlayer").) { return true; }
        return false;
    }

    public override void _Process(float delta)
    {
        Position = Position.LinearInterpolate(
            new Vector2(
                targetPosition.x * View.TILESIZE.x,
                targetPosition.y * View.TILESIZE.y
            ),
            1 - Mathf.Pow(1-0.3f, delta * 60f)
        );

        // TODO: Temporary hiding of entities. Should be model's responsibility to show/hide
        if (seen)
        {
            this.Modulate = this.Modulate.LinearInterpolate(Colors.White, 1 - Mathf.Pow(1-0.1f, delta * 60f));
        }
        else
        {
            this.Modulate = this.Modulate.LinearInterpolate(Colors.Transparent, 1 - Mathf.Pow(1-0.1f, delta * 60f));
        }
        // End TODO
    }
}
