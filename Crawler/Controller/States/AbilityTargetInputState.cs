using System;
using Godot;

public class AbilityTargetInputState : InputState
{
    (int x, int y) playerPos;
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

        // for (int dx = -(int)(action.Range.max + 0.5f); dx <= action.Range.max; dx++)
        // {
        //     for (int dy = -(int)(action.Range.max + 0.5f); dy <= action.Range.max; dy++)
        //     {
        //         // todo, make line of sight check.
        //         float dist = GridHelper.Distance(dx, dy);
        //         if (action.Range.min > dist) {continue;}
        //         if (dist > action.Range.max) {continue;}

        //         // todo: magic number 1.
        //         attackRange.SetCell(playerPos.x + dx, playerPos.y + dy, 1);
        //     }
        // }
    }

    public override void HandleInput(Crawler crawler, InputEvent ev)
    {
        foreach ((string name, (int x, int y) dir) tuple in DIRECTIONS)
        {
            if (ev.IsActionPressed(tuple.name))
            {
                cursor.targetPosition.x += tuple.dir.x;
                cursor.targetPosition.y += tuple.dir.y;
                RangeRefresh(crawler);
            }
        }

        if (ev is InputEventMouse evMouse)
        {
            // is it possible to get it from the thing instead?
            Vector2 mousePos = crawler.GetGlobalMousePosition();
            // Temporary.
            int previousX = cursor.targetPosition.x;
            int previousY = cursor.targetPosition.y;
            cursor.targetPosition.x = Mathf.RoundToInt(mousePos.x / View.TILESIZE.x);
            cursor.targetPosition.y = Mathf.RoundToInt(mousePos.y / View.TILESIZE.y);
            if (previousX != cursor.targetPosition.x || previousY != cursor.targetPosition.y)
            {
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
        Predicate<(int x, int y)> blocksAttack = x => walls.GetCell(x.x, x.y) >= 0;

        TileMap attackRange = crawler.GetNode("View").FindNode("AttackRange") as TileMap;
        attackRange.Clear();

        foreach ((int x, int y) in VisibilityTrie.FieldOfView(playerPos, blocksAttack, action.Range.max))
        // (x => false, action.Range.max, (cursor.targetPosition.x - playerPos.x, cursor.targetPosition.y - playerPos.y), 90))
        {
            float dist = GridHelper.Distance(x - playerPos.x, y - playerPos.y);
            if (action.Range.min > dist) { continue; }
            attackRange.SetCell(x, y, 1);
        }

        switch (action.TargetingType)
        {
            case TargetingType.Cone cone: RefreshCone(cone, attackRange, blocksAttack); break;
            case TargetingType.Smite smite: RefreshSmite(smite, attackRange, blocksAttack); break;
            case TargetingType.Ray shot: RefreshShot(shot, attackRange, blocksAttack); break;
            default:
                break;
        }
    }

    private void RefreshCone(TargetingType.Cone cone, TileMap attackRange, Predicate<(int, int)> blocksAttack)
    {
        foreach ((int x, int y) in VisibilityTrie.ConeOfView(playerPos, blocksAttack, action.Range.max, (cursor.targetPosition.x - playerPos.x, cursor.targetPosition.y - playerPos.y), cone.sectorDegrees))
        {
            attackRange.SetCell(x, y, 3);
        }
    }

    private void RefreshSmite(TargetingType.Smite smite, TileMap attackRange, Predicate<(int, int)> blocksAttack)
    {
        foreach ((int x, int y) in VisibilityTrie.FieldOfView(cursor.targetPosition, blocksAttack, smite.radius))
        {
            attackRange.SetCell(x, y, 3);
        }
    }

    private void RefreshShot(TargetingType.Ray ray, TileMap attackRange, Predicate<(int, int)> blocksAttack)
    {
        foreach ((int x, int y) in GridHelper.RayThrough(playerPos, cursor.targetPosition))
        {
            float dist = GridHelper.Distance(x - playerPos.x, y - playerPos.y);
            if (dist > action.Range.max) { break; }
            if (blocksAttack((x, y))) { break; }
            attackRange.SetCell(x, y, 3);
            if (ray.stopAtTarget && x == cursor.targetPosition.x && y == cursor.targetPosition.y) { break; }
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
    }
}