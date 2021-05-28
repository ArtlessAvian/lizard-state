using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    public Model model;
    public View View
    {
        get { return GetNode<View>("View"); }
    }

    private bool notPlayerTurn = true;

    public override void _EnterTree()
    {
        // Failsafe.
        if (model is null)
        {
            // EditorGenerator gen = new EditorGenerator("res://Crawler/Maps/BigTest.tscn");
            EditorGenerator gen = new EditorGenerator("res://Crawler/Maps/CrazyNoisy.tscn");
            // RandomGenerator gen = new RandomGenerator();
            model = gen.Generate(View.eventQueue);
        }
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
