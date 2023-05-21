using System;
using Godot;

public class ActionTargetInputState : InputState
{
    AbsolutePosition playerPos;
    Cursor cursor;
    internal Action action;

    public override void Enter(Crawler crawler)
    {
        playerPos = crawler.Model.GetPlayer().position;

        cursor = crawler.GetNode<Cursor>("Cursor");
        cursor.targetPosition = playerPos;
        cursor.SnapToTarget();
        cursor.Show();

        RangeRefresh(crawler);

        (crawler.FindNode("TargetingInfo") as Control).Show();
        (crawler.FindNode("TargetingInfoLabel") as Label).Text = GetActionName(action);
    }

    public override void HandleInput(Crawler crawler, InputEvent ev)
    {
        foreach ((string name, Vector2i dir) tuple in DIRECTIONS)
        {
            if (ev.IsActionPressed(tuple.name))
            {
                cursor.targetPosition += tuple.dir;
                RangeRefresh(crawler);
            }
        }

        if (ev is InputEventMouse evMouse)
        {
            // is it possible to get it from the thing instead?
            Vector2 mousePos = crawler.GetGlobalMousePosition();
            AbsolutePosition newPosition = new AbsolutePosition(
                Mathf.RoundToInt(mousePos.x / View.TILESIZE.x),
                Mathf.RoundToInt(mousePos.y / View.TILESIZE.y)
            );
            if (cursor.targetPosition != newPosition)
            {
                cursor.targetPosition = newPosition;
                RangeRefresh(crawler);
            }
        }

        if (ev.IsActionPressed("ui_accept"))
        {
            Select(crawler);
        }

        if (ev is InputEventMouseButton evMouseButton)
        {
            if (evMouseButton.ButtonIndex == (int)ButtonList.Left && evMouseButton.IsPressed())
            {
                Select(crawler);
            }
        }

        if (ev.IsActionPressed("ui_cancel"))
        {
            crawler.ResetState();
        }
    }

    private void RangeRefresh(Crawler crawler)
    {
        TileMap walls = crawler.GetNode("View").FindNode("Walls") as TileMap;
        Predicate<AbsolutePosition> blocksAttack = x => walls.GetCell(x.x, x.y) >= 0;

        TileMap attackRange = crawler.GetNode("View").FindNode("AttackRange") as TileMap;
        attackRange.Clear();

        foreach (AbsolutePosition tile in action.TargetingType.GetFullRange(playerPos, blocksAttack))
        {
            attackRange.SetCell(tile.x, tile.y, 1);
        }

        foreach (AbsolutePosition tile in action.TargetingType.GetInfoTiles(playerPos, cursor.targetPosition, blocksAttack))
        {
            attackRange.SetCell(tile.x, tile.y, 4);
        }

        foreach (AbsolutePosition tile in action.TargetingType.GetAffectedTiles(playerPos, cursor.targetPosition, blocksAttack))
        {
            attackRange.SetCell(tile.x, tile.y, 3);
        }
    }

    private void Select(Crawler crawler)
    {
        action.SetTarget(cursor.targetPosition);
        crawler.View.ModelSync();
        bool success = crawler.Model.SetPlayerAction(action);
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

        (crawler.FindNode("TargetingInfo") as Control).Hide();
    }

    private string GetActionName(Action action)
    {
        if (!action.ResourceName.Empty())
        {
            return action.ResourceName;
        }
        if (!action.ResourcePath.Empty())
        {
            return action.ResourcePath;
        }
        if (action.GetScript() is Resource script && !script.ResourcePath.Empty())
        {
            return script.ResourcePath;
        }
        return "Something";
    }
}