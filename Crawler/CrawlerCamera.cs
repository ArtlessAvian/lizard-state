using Godot;
using System;

public class CrawlerCamera : Camera2D
{
    public Node2D focus;

    public CrawlerCamera()
    {
        focus = this;
    }

    public override void _Process(float delta)
    {
        this.Position = focus.Position;
        this.ForceUpdateScroll();
    }
}
