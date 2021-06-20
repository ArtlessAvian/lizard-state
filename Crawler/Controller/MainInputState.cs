using Godot;
using Godot.Collections;

public class MainInputState : InputState
{
    Dictionary temp;

    public override void Input(Crawler crawler, InputEvent ev)
    {
        if (ev.IsActionPressed("quicksave", false))
        {
            temp = crawler.Model.SaveToDictionary();
            GetTree().SetInputAsHandled();
            return;
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

            GetTree().SetInputAsHandled();
            return;
        }

        if (ev.IsActionPressed("menu_abilities", false))
        {
            crawler.ChangeState(this.GetNode<InputState>("Ability"));
            return;
        }

        foreach ((string name, (int, int) dir) tuple in DIRECTIONS)
        {
            if (ev.IsActionPressed(tuple.name, true))
            {
                bool success = MoveOrAttack(crawler, tuple.dir);
                crawler.notPlayerTurn = true;
            }
        }

        if (ev.IsActionPressed("exit_action"))
        {
            GD.Print("befafa");
            crawler.Model.DoPlayerAction(new ExitAction());
            crawler.notPlayerTurn = true;
        }
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

    public override void Enter(Crawler crawler)
    {

    }

    public override void Exit(Crawler crawler)
    {

    }
}
