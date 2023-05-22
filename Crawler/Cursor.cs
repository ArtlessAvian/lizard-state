using Godot;
using System;

public class Cursor : Sprite
{
    public AbsolutePosition targetPosition;

    public override void _Process(float delta)
    {
        if (!Visible) {return;}

        SnapToTarget();
        // Position = Position.LinearInterpolate(
        //     new Vector2(
        //         targetPosition.x * View.TILESIZE.x,
        //         targetPosition.y * View.TILESIZE.y
        //     ),
        //     1 - Mathf.Pow(1-0.3f, delta * 60f)
        // );
    }

    public void SnapToTarget()
    {
        Position = new Vector2(
            targetPosition.x * View.TILESIZE.x,
            targetPosition.y * View.TILESIZE.y
        );
    }
}
