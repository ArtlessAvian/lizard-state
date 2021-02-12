using Godot;
using System;
using System.Collections.Generic;

public partial class Actor : Node2D
{
    PackedScene damagePopupScene = GD.Load<PackedScene>("res://Crawler/View/DamagePopup.tscn");

    // MASSIVE if-elif chain.
    // Oof.
    public void PerformAsSubject(ModelEvent ev, List<Actor> roles)
    {
        stunned = false;
        AnimatedSprite aniSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        aniSprite.Frame = 0;
        // EmitSignal("Action", action);
        // EmitSignal(action, args);

        if (ev.action == "Move")
        {
            (int x, int y) cast = ((int x, int y))ev.args;
            FaceDirection(cast.x - targetPosition.x, cast.y - targetPosition.y);
            targetPosition = cast;
        }
        else if (ev.action == "Swap")
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
        else if (ev.action == "Damaged")
        {
            AttackResult result = (AttackResult)ev.args;
            health -= result.damage;

            TextureProgress healthbar = GetNode<TextureProgress>("HealthBar");
            healthbar.Value = health;

            Label popup = (Label)damagePopupScene.Instance();
            popup.Text = $"-{result.damage}";
            this.GetNode("DamagePopups").AddChild(popup);

            AnimationPlayer animation = GetNode<AnimationPlayer>("AnimationPlayer");
            animation.Play("Hurt");
        }
        else if (ev.action == "Stunned")
        {
            AnimationPlayer animation = GetNode<AnimationPlayer>("AnimationPlayer");
            animation.Queue("Stunned");            
        }
        else if (ev.action == "Downed")
        {
            AnimationPlayer animation = GetNode<AnimationPlayer>("AnimationPlayer");
            animation.Queue("Downed");
        }
        else
        {
            GD.PrintErr("Unhandled subject command! ", ev.action);
        }
    }

    public void PerformAsObject(ModelEvent ev, List<Actor> roles)
    {
        if (ev.action == "Swap")
        {
            (int x, int y) cast = ((int x, int y))ev.args;
            FaceDirection(cast.x - targetPosition.x, cast.y - targetPosition.y);
            targetPosition = cast;
        }
        else if (ev.action == "Attack")
        {
            // (int x, int y) otherPosition = roles[ev.subject].targetPosition;
            // FaceDirection(otherPosition.x - targetPosition.x, otherPosition.y - targetPosition.y);
            
            // AttackResult roll = (AttackResult)ev.args;
            // health -= roll.damage;
            
            // Label popup = GetNode<Label>("DamagePopup");
            // popup.Text = $"-{roll.damage}";

            // TextureProgress healthbar = GetNode<TextureProgress>("HealthBar");
            // healthbar.Value = health;

            // AnimationPlayer animation = GetNode<AnimationPlayer>("AnimationPlayer");
            // if (health <= 0)
            // {
            //     animation.Play("Downed");
            // }
            // else
            // {
            //     // Bug: should stay stunned, but can't tell state!
            //     if (roll.damage > 0)
            //     {
            //         stunned |= roll.hit;
            //         animation.Play(stunned ? "Stunned" : "Hurt");
            //     }
            // }
        }
        else
        {
            GD.PrintErr("Unhandled object command! ", ev.action);
        }
    }
}
