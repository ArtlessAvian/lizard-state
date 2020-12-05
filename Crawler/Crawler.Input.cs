using Godot;
using System;

public partial class Crawler : Node2D
{
    public override void _Input(InputEvent ev)
    {
        if (ev.IsActionPressed("move_up", true))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((0, -1)));
            notPlayerTurn = true;
        }
        if (ev.IsActionPressed("move_down", true))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((0, 1)));
            notPlayerTurn = true;
        }
        if (ev.IsActionPressed("move_left", true))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((-1, 0)));
            notPlayerTurn = true;
        }
        if (ev.IsActionPressed("move_right", true))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((1, 0)));
            notPlayerTurn = true;
        }
        if (ev.IsActionPressed("move_upleft", true))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((-1, -1)));
            notPlayerTurn = true;
        }
        if (ev.IsActionPressed("move_upright", true))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((1, -1)));
            notPlayerTurn = true;
        }
        if (ev.IsActionPressed("move_downleft", true))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((-1, 1)));
            notPlayerTurn = true;
        }
        if (ev.IsActionPressed("move_downright", true))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((1, 1)));
            notPlayerTurn = true;
        }
    }
}
