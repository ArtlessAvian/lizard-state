using Godot;
using System;
using System.Collections.Generic;

// Like a ViewModel. Also, a pile of callbacks for the View.
public partial class Actor : Node2D
{
    [Signal]
    delegate void attack_active();

    public Entity role;

    [Export] public Vector2 tilePosition;
    public Vector2 lerpPosition;
    private SceneTreeTween movementTween = null;
    public static float snappiness = 0.3f;
    // from 0-1, fraction of the way to lerp to target per 60th of a second

    // Degrees: 0 is right, + is cw.
    [Export] public float facingDir;
    [Export] public float facingOffset;

    [Export] public Vector2 animationArg; // In Tiles
    [Export] public float spriteLerp;
    [Export] public float spriteZ;
    [Export] public bool animationInterruptible = false;
    public float timeStop = 0;

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
        tilePosition = new Vector2((int)role.position.x, (int)role.position.y);
        lerpPosition = tilePosition;
        // Position = tilePosition * View.TILESIZE; // elementwise

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

        // // sprite stuff
        // if (health <= 0) { this.Visible = false; }
        // else
        // {
        //     // AnimationPlayer aniPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        //     // aniPlayer.Queue("Reset");
        // }

        status?.Call("set_energy", role.energy, 10);

        stunned = role.stunned;
        AnimatedSprite aniSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        // aniSprite.Frames = GD.Load<SpriteFrames>($"res://Crawler/View/ActorData/{role.species.ResourceName}.tres");
        if (role.downed)
        {
            aniSprite.Frame = 3;
        }
        else if (role.stunned)
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

    public void GoToPosition(Vector2 tilePos, float speed)
    {
        Vector2 oldPosition = this.tilePosition;

        movementTween = CreateTween();
        // movementTween.SetProcessMode(Tween.TweenProcessMode.Physics);
        movementTween.TweenProperty(
                this, "lerpPosition", new Vector2(tilePos),
                GridHelper.Distance((int)(tilePos.x - oldPosition.x), (int)(tilePos.y - oldPosition.y)) / speed
            );
        movementTween.Play();

        this.tilePosition = tilePos;
    }

    private void FaceDirection(Vector2 dir)
    {
        if (dir == Vector2.Zero) { return; }
        AnimatedSprite sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Set("facing_dir", Mathf.Rad2Deg(dir.Angle()));
    }

    // private void FacePosition((int x, int y) position)
    // {
    //     FaceDirection(position.x - targetPosition.x, position.y - targetPosition.y);
    // }

    public void FacePosition(Vector2 position)
    {
        FaceDirection(position - tilePosition);
    }

    private void SetAnimationTarget(Vector2 tileOffset)
    {
        AnimatedSprite sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        sprite.Set("animation_target", tileOffset);
    }

    public bool IsAnimating()
    {
        // "bool?" type, so "== true" is needed.
        if (movementTween?.IsRunning() == true) { return true; }
        if (Math.Abs(tilePosition.x - Position.x / View.TILESIZE.x) > 0.01) { return true; }
        if (Math.Abs(tilePosition.y - Position.y / View.TILESIZE.y) > 0.01) { return true; }

        if (!animationInterruptible && GetNode<AnimationPlayer>("AnimationPlayer").IsPlaying()) { return true; }
        // if (GetNode<AnimationPlayer>("AnimationPlayer").) { return true; }
        return false;
    }

    public override void _Process(float delta)
    {
        if (timeStop > 0)
        {
            timeStop -= delta;
            if (timeStop > 0)
            {
                GetNode<AnimationPlayer>("AnimationPlayer").PlaybackSpeed = 0;
                return;
            }
            delta = -timeStop;
        }

        GetNode<AnimationPlayer>("AnimationPlayer").PlaybackSpeed = 1;

        Position = Position.LinearInterpolate(
            lerpPosition * View.TILESIZE,
            1 - Mathf.Pow(1 - snappiness, delta * 60f)
        );

        // TODO: Temporary hiding of entities.
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
