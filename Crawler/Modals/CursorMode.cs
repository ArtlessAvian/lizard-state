using Godot;
using System;

public class CursorMode : Control
{
    [Signal]
    public delegate void Select(int x, int y);
    [Signal]
    public delegate void Moved(int x, int y);

    [Export]
    NodePath cursorSprite;
    (int x, int y) position;

    public override void _Ready()
    {
        this.Hide();
        this.GetNode<Node2D>(cursorSprite)?.Hide();
        this.SetProcessInput(false);
    }

    public void Enter((int x, int y) position)
    {
        this.Show();
        this.GetNode<Node2D>(cursorSprite)?.Show();
        this.SetProcessInput(true);

        this.position = position;
        MoveCursor();
    }

    private (string, (int, int))[] directions = {
        ("move_up", (0, -1)),
        ("move_down", (0, 1)),
        ("move_left", (-1, 0)),
        ("move_right", (1, 0)),
        ("move_upleft", (-1, -1)),
        ("move_upright", (1, -1)),
        ("move_downleft", (-1, 1)),
        ("move_downright", (1, 1)),
        ("move_wait", (0, 0))
    };

    public override void _Input(InputEvent ev)
    {
        foreach ((string name, (int x, int y) dir) tuple in directions)
        {
            if (ev.IsActionPressed(tuple.name, true))
            {
                position.x += tuple.dir.x;
                position.y += tuple.dir.y;
                MoveCursor();
            }
        }

        if (ev.IsActionPressed("ui_accept"))
        {
            this.EmitSignal("Select", position.x, position.y);
        }
    }

    public void MoveCursor()
    {
        Node2D cursor = this.GetNodeOrNull<Node2D>(cursorSprite);
        if (cursor != null)
        {
            cursor.Position = new Vector2((position.x + 0.5f) * View.TILESIZE.x, (position.y + 0.5f) * View.TILESIZE.y);
        }
        
        this.EmitSignal("Moved", position.x, position.y);
    }

    public void Exit()
    {
        this.Hide();
        this.GetNode<Node2D>(cursorSprite)?.Hide();
        this.SetProcessInput(false);
    }
}
