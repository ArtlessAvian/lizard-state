using System;
using Godot;
using LizardState.Engine;

public class ActionTargetInputState : InputState
{
    AbsolutePosition playerPos;
    Cursor cursor;
    internal CrawlAction action;

    public override void Enter(Crawler crawler)
    {
        playerPos = crawler.Model.GetPlayer().position;

        cursor = crawler.GetNode<Cursor>("Cursor");
        cursor.targetPosition = playerPos;
        cursor.SnapToTarget();
        cursor.Show();

        RangeRefresh(crawler);

        (crawler.FindNode("TargetingInfo") as Control).Show();
        (crawler.FindNode("TargetingInfoLabel") as Label).Text = GetActionName(action) + "\n" + action.TargetingType.GetType().Name;
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

        foreach (AbsolutePosition tile in VisibilityTrie.FieldOfView(playerPos, blocksAttack, action.Range.max))
        // (x => false, action.Range.max, (cursor.targetPosition.x - playerPos.x, cursor.targetPosition.y - playerPos.y), 90))
        {
            float dist = GridHelper.Distance(tile - playerPos);
            if (action.Range.min > dist) { continue; }
            attackRange.SetCell(tile.x, tile.y, 1);
        }

        switch (action.TargetingType)
        {
            case TargetingType.Cone cone: RefreshCone(cone, attackRange, blocksAttack); break;
            case TargetingType.Smite smite: RefreshSmite(smite, attackRange, blocksAttack); break;
            case TargetingType.Ray shot: RefreshShot(shot, attackRange, blocksAttack); break;
            case TargetingType.Line line: RefreshLine(line, attackRange, blocksAttack); break;
            default:
                break;
        }
    }

    private void RefreshCone(TargetingType.Cone cone, TileMap attackRange, Predicate<AbsolutePosition> blocksAttack)
    {
        foreach (AbsolutePosition tile in VisibilityTrie.ConeOfView(playerPos, blocksAttack, action.Range.max, cursor.targetPosition - playerPos, cone.sectorDegrees))
        {
            attackRange.SetCell(tile.x, tile.y, 3);
        }
    }

    private void RefreshSmite(TargetingType.Smite smite, TileMap attackRange, Predicate<AbsolutePosition> blocksAttack)
    {
        foreach (AbsolutePosition tile in VisibilityTrie.FieldOfView(cursor.targetPosition, blocksAttack, smite.radius))
        {
            attackRange.SetCell(tile.x, tile.y, 3);
        }
    }

    private void RefreshShot(TargetingType.Ray ray, TileMap attackRange, Predicate<AbsolutePosition> blocksAttack)
    {
        foreach (AbsolutePosition tile in GridHelper.RayThrough(playerPos, cursor.targetPosition))
        {
            float dist = GridHelper.Distance(tile, playerPos);
            if (dist > action.Range.max) { break; }
            if (blocksAttack(tile)) { break; }
            attackRange.SetCell(tile.x, tile.y, 3);
            if (ray.stopAtTarget && tile == cursor.targetPosition) { break; }
        }
    }

    private void RefreshLine(TargetingType.Line line, TileMap attackRange, Predicate<AbsolutePosition> blocksAttack)
    {
        foreach (AbsolutePosition tile in GridHelper.LineBetween(playerPos, cursor.targetPosition))
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

    private string GetActionName(CrawlAction action)
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