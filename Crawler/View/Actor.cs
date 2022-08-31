using Godot;
using System;
using System.Collections.Generic;

// Like a ViewModel. Also, a pile of callbacks for the View.
[Tool]
public partial class Actor : Node2D
{
    [Signal]
    delegate void attack_active();

    public Entity role;

    [Export] public Vector2 targetPosition;
    [Export] public Vector2 animationArg; // In Tiles
    [Export] public float spriteLerp;
    [Export] public float spriteZ;

    int health = 0;
    bool stunned = false;
    bool seen = false;

    // Other elements.
    Node status = null;

    // TODO: Temporary
    public string displayName;

    public void ActAs(Entity role)
    {
        this.role = role;
        TextureProgress healthbar = GetNode<TextureProgress>("HealthBar");
        healthbar.MaxValue = role.species.maxHealth;
        ModelSync();
    }

    public void ModelSync(int? viewTime = null)
    {
        targetPosition = new Vector2((int)role.position.x, (int)role.position.y);
        // Position = new Vector2(
        //     targetPosition.x * View.TILESIZE.x,
        //     targetPosition.y * View.TILESIZE.y
        // );
        Position = targetPosition * View.TILESIZE; // elementwise

        health = role.health;
        status?.Call("set_health", role.health, role.species.maxHealth);

        TextureProgress healthbar = GetNode<TextureProgress>("HealthBar");
        healthbar.Value = role.health;

        // idk, its kind of ugly
        if (viewTime is int viewTimeeeee)
        {
            TextureProgress combobar = GetNode<TextureProgress>("ComboBar");
            combobar.Value = role.nextMove - viewTimeeeee;
        }

        // sprite stuff
        if (health <= 0) { this.Visible = false; }
        else
        {
            // AnimationPlayer aniPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            // aniPlayer.Queue("Reset");
        }

        status?.Call("set_energy", role.energy, 10);

        stunned = role.stunned;
        AnimatedSprite aniSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        // aniSprite.Frames = GD.Load<SpriteFrames>($"res://Crawler/View/ActorData/{role.species.ResourceName}.tres");
        if (role.stunned)
        {
            aniSprite.Frame = 1;
        }
        else if (role.queuedAction is ReachAttackFollowup)
        {
            aniSprite.Frame = 2;
        }
        else
        {
            aniSprite.Frame = 0;
        }

        // TODO: Temporary
        displayName = role.species.displayName;

        seen = role.visibleToPlayer;
    }

    private void FaceDirection(Vector2 dir)
    {
        if (dir == Vector2.Zero) { return; }

        AnimatedSprite sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        int frame = sprite.Frame;
        if (Math.Abs(dir.y / dir.x) > 1)
        {
            sprite.Animation = dir.y > 0 ? "South" : "North";
        }
        else
        {
            sprite.Animation = "East";
            sprite.FlipH = dir.x < 0;
        }
        sprite.Frame = frame;
    }

    // private void FacePosition((int x, int y) position)
    // {
    //     FaceDirection(position.x - targetPosition.x, position.y - targetPosition.y);
    // }

    public void FacePosition(Vector2 position)
    {
        FaceDirection(position - targetPosition);
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
            1 - Mathf.Pow(1 - 0.3f, delta * 60f)
        );

        GetNode<Node2D>("AnimatedSprite").Position = animationArg.LimitLength(1) * View.TILESIZE * spriteLerp;
        GetNode<Node2D>("AnimatedSprite").Position += Vector2.Up * spriteZ;

        // TODO: Temporary hiding of entities. Should be model's responsibility to show/hide
        if (seen || Engine.EditorHint)
        {
            this.Modulate = this.Modulate.LinearInterpolate(Colors.White, 1 - Mathf.Pow(1 - 0.1f, delta * 60f));
        }
        else
        {
            this.Modulate = this.Modulate.LinearInterpolate(Colors.Transparent, 1 - Mathf.Pow(1 - 0.1f, delta * 60f));
        }
        // End TODO
    }
}
