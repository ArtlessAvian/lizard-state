using Godot;
using Godot.Collections;

public class MainInputState : InputState
{
    Dictionary temp;

    Cursor cursor;
    Camera2D camera;

    public override void Enter(Crawler crawler)
    {
        cursor = crawler.GetNode<Cursor>("Cursor");
        camera = crawler.View.GetNode<Camera2D>("Camera2D");
    }

    public override void HandleInput(Crawler crawler, InputEvent ev)
    {
        if (this.DebugInput(crawler, ev))
        { GetTree().SetInputAsHandled(); return; }

        if (this.TransitionInput(crawler, ev))
        { GetTree().SetInputAsHandled(); return; }
        
        if (this.LogicInput(crawler, ev))
        { GetTree().SetInputAsHandled(); return; }
    }

    public bool DebugInput(Crawler crawler, InputEvent ev)
    {
        if (ev.IsActionPressed("quicksave", false))
        {
            temp = crawler.Model.SaveToDictionary();
            return true;
        }

        if (ev.IsActionPressed("quickload", false))
        {        
            // TODO: Fix.
            PackedScene viewScene = GD.Load<PackedScene>("res://Crawler/View/View.tscn");
            View view = (View)viewScene.Instance();

            View old = crawler.GetNode<View>("View");
            crawler.RemoveChild(old);
            old.QueueFree();
            crawler.AddChild(view);

            LoadedGenerator gen = new LoadedGenerator(temp);
            gen.Generate(crawler.Model);

            return true;
        }

        if (Input.IsKeyPressed((int)KeyList.F1))
        {
            crawler.View.GetNode("Map/Floors").Set("tile_data", crawler.Model.Map.Get("tile_data"));
            return true;
        }

        return false;
    }

    private bool TransitionInput(Crawler crawler, InputEvent ev)
    {
        if (ev.IsActionPressed("menu_abilities", false))
        {
            crawler.ChangeState(this.GetNode<InputState>("Ability"));
            return true;
        }

        if (ev.IsActionPressed("look") || ev.IsActionPressed("ui_cancel"))
        {
            crawler.ChangeState(this.GetNode<InputState>("Look"));
            return true;
        }

        return false;
    }

    private bool LogicInput(Crawler crawler, InputEvent ev)
    {
        foreach ((string name, (int, int) dir) tuple in DIRECTIONS)
        {
            if (ev.IsActionPressed(tuple.name, true))
            {
                bool success = MoveOrAttack(crawler, tuple.dir);
                crawler.notPlayerTurn = true;
                return true;
            }
        }

        if (ev.IsActionPressed("exit_action"))
        {
            GD.Print("befafa");
            crawler.Model.DoPlayerAction(new ExitAction());
            crawler.notPlayerTurn = true;
            return true;
        }

        if (ev is InputEventMouse evMouse)
        {
            // is it possible to get it from the thing instead?
            Vector2 mousePos = crawler.GetGlobalMousePosition();
            // Temporary.
            cursor.targetPosition.x = Mathf.RoundToInt(mousePos.x / View.TILESIZE.x);
            cursor.targetPosition.y = Mathf.RoundToInt(mousePos.y / View.TILESIZE.y);
            cursor.Show();

            // Draw a path between the player and the target.
        }

        if (ev is InputEventMouseButton evMouseButton)
        {
            if (evMouseButton.ButtonIndex == (int)ButtonList.Left && evMouseButton.IsPressed())
            {
                Vector2 mousePos = crawler.GetGlobalMousePosition();
                (int x, int y) targetPosition;
                targetPosition.x = Mathf.RoundToInt(mousePos.x / View.TILESIZE.x);
                targetPosition.y = Mathf.RoundToInt(mousePos.y / View.TILESIZE.y);
                
                GD.Print(targetPosition);
                // If the target is one tile away,
                    // Try Attacking
                    // Try Moving
                // Path to the target.
            } 
        }
        
        return false;
    }

    private bool MoveOrAttack(Crawler crawler, (int x, int y) direction)
    {
        Entity player = crawler.Model.GetPlayer();
        (int x, int y) offset = (player.position.x + direction.x, player.position.y + direction.y);
        Entity entityAt = crawler.Model.GetEntityAt(offset);

        if (entityAt != null && entityAt.team != player.team)
        {
            return crawler.Model.DoPlayerAction(new AttackAction(player.species.bumpAttack).Target(offset));
        }
        return crawler.Model.DoPlayerAction(new MoveAction().Target(offset));
    }

    public override void Exit(Crawler crawler)
    {

    }
}
