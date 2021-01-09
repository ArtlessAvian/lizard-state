using Godot;
using System;
using System.Collections.Generic;

// Like a ViewModel. Also, a pile of callbacks for the View.
public partial class Actor : Node2D
{
    (int x, int y) targetPosition;
    string roleName;
    int health = 0;

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
    }

    public void PerformAsSubject(ModelEvent ev, Dictionary<Entity, Actor> roles)
    {
        // EmitSignal("Action", action);
        // EmitSignal(action, args);

        if (ev.action == "Move")
        {
            (int x, int y) cast = ((int x, int y))ev.args;
            FaceDirection(cast.x - targetPosition.x, cast.y - targetPosition.y);
            targetPosition = cast;
        }
        if (ev.action == "Swap")
        {
            (int x, int y) otherPosition = roles[ev.obj].targetPosition;
            FaceDirection(otherPosition.x - targetPosition.x, otherPosition.y - targetPosition.y);
            targetPosition = otherPosition;
        }
        
        else if (ev.action == "Attack")
        {
            (int x, int y) otherPosition = roles[ev.obj].targetPosition;
            FaceDirection(otherPosition.x - targetPosition.x, otherPosition.y - targetPosition.y);
            AnimationPlayer animation = GetNode<AnimationPlayer>("AnimationPlayer");
            animation.Play("Attack");
        }
    }

    public void PerformAsObject(ModelEvent ev, Dictionary<Entity, Actor> roles)
    {
        if (ev.action == "Swap")
        {
            (int x, int y) cast = ((int x, int y))ev.args;
            FaceDirection(cast.x - targetPosition.x, cast.y - targetPosition.y);
            targetPosition = cast;
        }
        if (ev.action == "Attack")
        {
            (int x, int y) otherPosition = roles[ev.subject].targetPosition;
            FaceDirection(otherPosition.x - targetPosition.x, otherPosition.y - targetPosition.y);
            
            AttackAction.AttackResult roll = (AttackAction.AttackResult)ev.args;
            health -= roll.damage;
            
            Label popup = GetNode<Label>("DamagePopup");
            popup.Text = $"-{roll.damage}";

            TextureProgress healthbar = GetNode<TextureProgress>("HealthBar");
            healthbar.Value = health;

            AnimationPlayer animation = GetNode<AnimationPlayer>("AnimationPlayer");
            if (health <= 0)
            {
                animation.Play("Downed");
            }
            else
            {
                animation.Play(roll.crit ? "Stunned" : "Hurt");
            }
        }
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
