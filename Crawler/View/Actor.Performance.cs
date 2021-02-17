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
        else if (ev.action == "StartAttack")
        {
            (int x, int y) direction = ((int x, int y))ev.args;
            FaceDirection(direction.x, direction.y);
            AnimationPlayer animation = GetNode<AnimationPlayer>("AnimationPlayer");
            animation.Play("Attack");
        }
        else if (ev.action == "Unstun")
        {
            stunned = false;
            AnimatedSprite aniSprite = GetNode<AnimatedSprite>("AnimatedSprite");
            aniSprite.Frame = 0;
        }
        else if (ev.action == "Downed")
        {
            AnimationPlayer animation = GetNode<AnimationPlayer>("AnimationPlayer");
            animation.Queue("Downed");
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
        else if (ev.action == "Hit")
        {
            AttackResult result = (AttackResult)ev.args;
            health -= result.damage;
            stunned |= result.stuns;

            TextureProgress healthbar = GetNode<TextureProgress>("HealthBar");
            healthbar.Value = health;

            Label popup = (Label)damagePopupScene.Instance();
            popup.Text = $"-{result.damage}";
            this.GetNode("DamagePopups").AddChild(popup);

            AnimationPlayer animation = GetNode<AnimationPlayer>("AnimationPlayer");
            animation.Play(stunned ? "Stunned" : "Hurt");
        }
        else if (ev.action == "Miss")
        {
            Label popup = (Label)damagePopupScene.Instance();
            popup.Text = $"Miss";
            this.GetNode("DamagePopups").AddChild(popup);
        }
        // else
        // {
        //     GD.PrintErr("Unhandled object command! ", ev.action);
        // }
    }
}
