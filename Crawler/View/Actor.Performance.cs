using Godot;
using Godot.Collections;
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
            Vector2 cast = (Vector2)ev.args;
            FacePosition(cast);
            targetPosition = cast;
        }
        else if (ev.action == "Swap")
        {
            Vector2 otherPosition = roles[ev.obj].targetPosition;
            FacePosition(otherPosition);
            targetPosition = otherPosition;
        }
        else if (ev.action == "StartAttack")
        {
            Vector2 target = (Vector2)ev.args;
            FacePosition(target);
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
        else if (ev.action == "Unsee")
        {
            this.seen = false;
            GD.Print("unsee");
        }
    }

    public void PerformAsObject(ModelEvent ev, List<Actor> roles)
    {
        if (ev.action == "Swap")
        {
            Vector2 cast = (Vector2)ev.args;
            FacePosition(cast);
            targetPosition = cast;
        }
        else if (ev.action == "Hit")
        {
            Dictionary result = (Dictionary)ev.args;
            health -= (int)result["damage"];
            stunned |= (bool)result["stuns"];

            TextureProgress healthbar = GetNode<TextureProgress>("HealthBar");
            healthbar.Value = health;

            Label popup = (Label)damagePopupScene.Instance();
            popup.Text = $"-{(int)result["damage"]}";
            popup.AddColorOverride("font_color", Color.Color8(168, 168, 168));
            if ((bool)result["stuns"])
            {
                popup.AddColorOverride("font_color", Color.Color8(255, 0, 0));
                popup.Text += "!";
            }
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
        else if (ev.action == "See")
        {
            this.seen = true;
            GD.Print("See!");
        }
        // else
        // {
        //     GD.PrintErr("Unhandled object command! ", ev.action);
        // }
    }
}
