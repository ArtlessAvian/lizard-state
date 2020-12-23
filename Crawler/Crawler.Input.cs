using Godot;
using Godot.Collections;
using System;

public partial class Crawler : Node2D
{
    Dictionary temp;

    public override void _Input(InputEvent ev)
    {
        if (ev.IsActionPressed("quicksave", false))
        {
            temp = model.SaveToDictionary();
        }
        if (ev.IsActionPressed("quickload", false))
        {
            PackedScene crawlerScene = GD.Load<PackedScene>("res://Crawler/Crawler.tscn");

            Crawler crawler = (Crawler)crawlerScene.Instance();
        
            LoadedGenerator gen = new LoadedGenerator(temp);
            crawler.model = gen.Generate(crawler.eventQueue);

            crawler.temp = temp; // temppppp

            GetTree().Root.AddChild(crawler);
            GetTree().CurrentScene = crawler;
            GetTree().Root.RemoveChild(this);
        }

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
        if (ev.IsActionPressed("move_wait", true))
        {
            model.DoPlayerAction(eventQueue, new MoveAction((0, 0)));
            notPlayerTurn = true;
        }
    }
}
