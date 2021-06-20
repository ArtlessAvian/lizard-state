using Godot;

public class LookInputState : InputState
{
    Camera2D camera;
    Cursor cursor;

    public override void Enter(Crawler crawler)
    {
        camera = crawler.View.GetNode<Camera2D>("Camera2D"); 
        cursor = crawler.GetNode<Cursor>("Cursor");
        
        camera.Zoom = Vector2.One;
        // cursor.Show();
    }

    public override void Exit(Crawler crawler)
    {
        camera.Zoom = Vector2.One / 2;
        camera.Offset = Vector2.Zero;

        // cursor.Hide();
    }

    public override void HandleInput(Crawler crawler, InputEvent ev)
    {
        foreach ((string name, (int x, int y) dir) tuple in DIRECTIONS)
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
