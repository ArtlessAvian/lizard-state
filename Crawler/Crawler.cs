using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    public View View
    {
        get { return GetNode<View>("View"); }
    }

    public Model model
    {
        get { return View.model;}
    } // TODO: just use View.model everywhere.

    private bool notPlayerTurn = true;

    public override void _Ready()
    {
        NoiseGenerator gen = new NoiseGenerator();
        gen.Generate(View.model);

        // View.ClearQueue();
        // View.playerActor = View.roles[0];
        // View.GetNode<CrawlerCamera>("Camera2D").focus = View.playerActor ?? (Node2D)View;
    }

    public override void _Process(float delta)
    {
        uint start = OS.GetTicksMsec();
        this.RunModel(start);

        if (Input.IsKeyPressed((int)KeyList.F1))
        {
            View.GetNode("Map/Floors").Set("tile_data", model.map.map.Get("tile_data"));
        }
    }

    private void RunModel(uint start)
    {
        while (notPlayerTurn) // and not timed out
        {
            if (!model.DoEntityAction())
            {
                // let the player move again.
                notPlayerTurn = false;
                break;
            }

            if (OS.GetTicksMsec() - start > 1000/120f)
            {
                GD.PrintErr("Timed out!");
                break;
            }

            // if model.done
            // generate new model
            // replace model
            // clear view
        }
    }
}
