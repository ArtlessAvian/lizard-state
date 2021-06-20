using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class Crawler : Node2D, InputStateMachine
{
    public View View
    {
        get { return GetNode<View>("View"); }
    }

    public Model Model
    {
        get { return GetNode<Model>("Model");}
    }

    public InputState activeInputState;
    public bool notPlayerTurn = true;

    public override void _Ready()
    {
        Model.NewEvent += View.eventQueue.Add; // So clean!!
        activeInputState = GetNode<InputState>("InputStates/Main");

        NoiseGenerator gen = new NoiseGenerator();
        gen.Generate(Model);
    }

    public override void _Process(float delta)
    {
        uint start = OS.GetTicksMsec();
        this.RunModel(start);

        if (Input.IsKeyPressed((int)KeyList.F1))
        {
            View.GetNode("Map/Floors").Set("tile_data", Model.Map.Get("tile_data"));
        }
    }

    private void RunModel(uint start)
    {
        while (notPlayerTurn) // and not timed out
        {
            if (!Model.DoEntityAction())
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

    public override void _UnhandledInput(InputEvent ev)
    {
        if (notPlayerTurn) { return; }
        if (View.eventQueue.Count > 0) { return; }
        foreach (Control p in FindNode("Modals").GetChildren())
        {
            if (p.Visible) { return; }
        }

        activeInputState.Input(this, ev);
    }

    public void ChangeState(InputState to)
    {
        activeInputState.Exit(this);
        activeInputState = to;
        activeInputState.Enter(this);
    }

    // public void ChangeState(string to)
    // {
    //     this.ChangeState((InputState)GetNode<InputState>("InputStates").FindNode(to));
    // }

    public void ResetState()
    {
        this.ChangeState(GetNode<InputState>("InputStates/Main"));
    }
}
