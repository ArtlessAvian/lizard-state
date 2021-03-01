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
            model = gen.Generate(View.eventQueue);
        }
    }

    public override void _Process(float delta)
    {
        uint start = OS.GetTicksMsec();
        this.RunModel(start);
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
        }
    }
}
