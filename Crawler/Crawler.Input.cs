using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    Dictionary temp;

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
        if (notPlayerTurn) { return; }
        if (eventQueue.Count > 0) { return; }

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

        foreach ((string name, (int, int) dir) tuple in directions)
        {
            if (ev.IsActionPressed(tuple.name, true))
            {
                bool success = MoveOrAttack(tuple.dir);
                // model.DoPlayerAction(eventQueue, new MoveAction(tuple.dir));
                notPlayerTurn = true;
            }
        }
    }

    private bool MoveOrAttack((int x, int y) direction)
    {
        Entity player = model.GetPlayer();
        Entity entityAt = model.GetEntityAt(player.position.x + direction.x, player.position.y + direction.y);
        if (entityAt != null && entityAt.team != player.team)
        {
            return model.DoPlayerAction(eventQueue, new AttackAction(direction));
        }
        return model.DoPlayerAction(eventQueue, new MoveAction(direction));
    }
}
