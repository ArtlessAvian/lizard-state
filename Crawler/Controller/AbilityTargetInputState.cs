using System;
using Godot;

public class AbilityTargetInputState : InputState
{
    (int x, int y) playerPos;
    Cursor cursor;
    internal Action action;

    public override void Enter(Crawler crawler)
    {
        playerPos = crawler.model.GetPlayer().position;

        cursor = crawler.GetNode<Cursor>("Cursor");
        cursor.targetPosition = playerPos;
        cursor.SnapToTarget();
        cursor.Show();

        TileMap attackRange = crawler.GetNode("View").FindNode("AttackRange") as TileMap;
        for (int dx = -(int)(action.Range.max + 0.5f); dx <= action.Range.max; dx++)
        {
            for (int dy = -(int)(action.Range.max + 0.5f); dy <= action.Range.max; dy++)
            {
                // todo, make line of sight check.
                float dist = GridHelper.Distance(dx, dy);
                if (action.Range.min > dist) {continue;}
                if (dist > action.Range.max) {continue;}

                // todo: magic number 1.
                attackRange.SetCell(playerPos.x + dx, playerPos.y + dy, 1);
            }
        }
    }

    public override void HandleInput(Crawler crawler, InputEvent ev)
    {
        foreach ((string name, (int x, int y) dir) tuple in DIRECTIONS)
        {
            if (ev.IsActionPressed(tuple.name))
            {
                cursor.targetPosition.x += tuple.dir.x;
                cursor.targetPosition.y += tuple.dir.y;
            }
        }

        if (ev is InputEventMouse evMouse)
        {
            // is it possible to get it from the thing instead?
            Vector2 mousePos = crawler.GetGlobalMousePosition();
            // Temporary.
            cursor.targetPosition.x = Mathf.RoundToInt(mousePos.x / View.TILESIZE.x);
            cursor.targetPosition.y = Mathf.RoundToInt(mousePos.y / View.TILESIZE.y);
        }

        if (ev.IsActionPressed("ui_accept"))
        {
            float distance = GridHelper.Distance(playerPos, cursor.targetPosition);
            GD.Print(distance);
            if (action.Range.min <= distance && distance <= action.Range.max)
            {
                Select(crawler);
            }
        }

        if (ev is InputEventMouseButton evMouseButton)
        {
            if (evMouseButton.ButtonIndex == (int)ButtonList.Left && evMouseButton.IsPressed())
            {
                float distance = GridHelper.Distance(playerPos, cursor.targetPosition);
                GD.Print(distance);
                if (action.Range.min <= distance && distance <= action.Range.max)
                {
                    Select(crawler);
                }
            }
        }

        if (ev.IsActionPressed("ui_cancel"))
        {
            crawler.ResetState();
        }
    }

    private void Select(Crawler crawler)
    {
        action.SetTarget(cursor.targetPosition);
        bool success = crawler.model.SetPlayerAction(action);
        crawler.notPlayerTurn = true;
        if (success)
        {
            crawler.ResetState();
        }
    }

    public override void Exit(Crawler crawler)
    {
        cursor.Hide();

        TileMap attackRange = crawler.GetNode("View").FindNode("AttackRange") as TileMap;
        attackRange.Clear();
    }
}