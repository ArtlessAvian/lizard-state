using Godot;
using LizardState.Engine;

public class LookInputState : InputState
{
    Camera2D camera;
    Cursor cursor;
    float oldZoom = 2;

    public override void Enter(Crawler crawler)
    {
        camera = crawler.View.GetNode<Camera2D>("Camera2D");
        cursor = crawler.GetNode<Cursor>("Cursor");

        oldZoom = camera.Zoom.x;
        camera.Zoom = Vector2.One;
        // cursor.Show();
    }

    public override void Exit(Crawler crawler)
    {
        camera.Zoom = Vector2.One * oldZoom;
        camera.Offset = Vector2.Zero;

        // cursor.Hide();
    }

    public override void HandleInput(Crawler crawler, InputEvent ev)
    {
        foreach ((string name, Vector2i dir) tuple in DIRECTIONS)
        {
            if (ev.IsActionPressed(tuple.name, true))
            {
                camera.Offset += new Vector2(tuple.dir.x * View.TILESIZE.x, tuple.dir.y * View.TILESIZE.y);
            }
        }

        if (ev.IsActionPressed("look") || ev.IsActionPressed("ui_cancel"))
        {
            crawler.ResetState();
        }
    }
}
