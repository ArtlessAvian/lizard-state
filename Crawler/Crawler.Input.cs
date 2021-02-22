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
    };

    private (string, (int, int))[] diagonalDirections = {
        ("move_upleft", (-1, -1)),
        ("move_upright", (1, -1)),
        ("move_downleft", (-1, 1)),
        ("move_downright", (1, 1)),
        ("move_wait", (0, 0))
    };

    public override void _UnhandledInput(InputEvent ev)
    {
        if (notPlayerTurn) { return; }
        if (eventQueue.Count > 0) { return; }
        foreach (Popup p in GetNode("UILayer/Modals").GetChildren())
        {
            if (p.Visible) { return; }
        }

        if (ev.IsActionPressed("quicksave", false))
        {
            temp = model.SaveToDictionary();
            GetTree().SetInputAsHandled();
            return;
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
            this.QueueFree();
            // GetTree().Root.RemoveChild(this);
            GetTree().SetInputAsHandled();
            return;
        }

        if (ev.IsActionPressed("menu_abilities", false))
        {
            GetNode<Popup>("UILayer/Modals/AbilitiesMenu").Popup_();
            return;
        }

        foreach ((string name, (int, int) dir) tuple in directions)
        {
            if (ev.IsActionPressed(tuple.name, false))
            {
                bool success = MoveOrAttack(tuple.dir);
                notPlayerTurn = true;
            }
        }
    }

    private void PollInput()
    {
        // if (!Input.IsActionPressed("modifier_diagonal"))
        // {
        //     foreach ((string name, (int, int) dir) tuple in directions)
        //     {
        //         if (Input.IsActionPressed(tuple.name))
        //         {
        //             bool success = MoveOrAttack(tuple.dir);
        //             notPlayerTurn = true;
        //         }
        //     }
        // }
        // else
        // {

        // }
    }

    private bool MoveOrAttack((int x, int y) direction)
    {
        Entity player = model.GetPlayer();
        Entity entityAt = model.GetEntityAt(player.position.x + direction.x, player.position.y + direction.y);
        if (entityAt != null && entityAt.team != player.team)
        {
            return model.DoPlayerAction(new AttackAction(direction));
        }
        return model.DoPlayerAction(new MoveAction(direction));
    }
}
