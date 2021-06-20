using Godot;

public class AbilityTargetInputState : InputState
{
    [Export]
    NodePath cursorPath;
    Cursor cursor;
    internal ActionTargeted action;

    public override void Enter(Crawler crawler)
    {
        cursor = this.GetNode<Cursor>(cursorPath);
        
        cursor.targetPosition = crawler.Model.GetPlayer().position;
        cursor.SnapToTarget();
        cursor.Show();
    }

    public override void Input(Crawler crawler, InputEvent ev)
    {
        foreach ((string name, (int x, int y) dir) tuple in DIRECTIONS)
        {
            if (ev.IsActionPressed(tuple.name))
            {
                cursor.targetPosition.x += tuple.dir.x;
                cursor.targetPosition.y += tuple.dir.y;
            }
        }

        if (ev.IsActionPressed("ui_accept"))
        {
            action.Target(cursor.targetPosition);
            bool success = crawler.Model.DoPlayerAction(action);
            if (success)
            {            
                crawler.ResetState();
            }
            return;
        }

        if (ev.IsActionPressed("ui_cancel"))
        {
            crawler.ResetState();
        }
    }

    public override void Exit(Crawler crawler)
    {
        this.GetNode<Node2D>(cursorPath)?.Hide();
    }

    public void aefajefk()
    {
        // // Check if valid
        // // Disconnect
        // CursorMode cursorMode = FindNode("Modals").GetNode<CursorMode>("CursorMode");
        // cursorMode.Disconnect("Select", this, "AbilityTargeted");
        // cursorMode.Exit();

        // // Do the move
        // actionTargeting.Target((x, y));
        // Model.DoPlayerAction(actionTargeting);
        // notPlayerTurn = true;
    }
}