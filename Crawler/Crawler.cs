using Godot;
using Godot.Collections;
using System;
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
        Model.Connect("NewEvent", View, "OnModelNewEvent");
        // Model.NewEvent += View.eventQueue.Add; // So clean!!

        activeInputState = GetNode<InputState>("InputStates/Main");
        activeInputState.Enter(this);

        // NoiseGenerator gen = new NoiseGenerator();
        EditorGenerator gen = new EditorGenerator("res://Crawler/Generators/Maps/MVP-Scaled.tscn");
        gen.Generate(Model);

        Model.CoolerApiEvent(-1, "Print", "[G]et the moss (green tiles) with the G key.");
        Model.CoolerApiEvent(-1, "Print", "Then leave the cave from where you entered.");

        if (GetViewport().Size.x >= 960 * 2)
        {
            ProjectSettings.SetSetting("display/window/stretch/shrink", (int)GetViewport().Size.x / 960);
        }
    }

    public override void _Process(float delta)
    {
        uint start = OS.GetTicksMsec();
        this.RunModel(start);
    }

    // Treat as daemon.
    private void RunModel(uint start)
    {
        // while (true) // treat as daemon (its not designed for that yet)
        while (notPlayerTurn) // and not timed out
        {
            bool success = Model.DoStep();
            if (!success)
            {
                // let the player move again.
                notPlayerTurn = false;
                View.queueSync = true;
                break;
            }

            // Uncomment if not lag testing (which should be always)
            // if (OS.GetTicksMsec() - start > 1000/120f)
            // {
            //     GD.PrintErr("Timed out!");
            //     break;
            // }

            // if model.done
            // generate new model
            // replace model
            // clear view
        }
        float frameTime = OS.GetTicksMsec() - start;
        if (frameTime > 1000/120f)
        {
            Print($"High frame time! ({frameTime} ms)");
        }
    }

    public override void _UnhandledInput(InputEvent ev)
    {
        if (notPlayerTurn) { return; }
        if (View.eventQueue.Count > 0) { return; }
        // foreach (Control p in FindNode("Modals").GetChildren())
        // {
        //     if (p.Visible) { return; }
        // }

        activeInputState.HandleInput(this, ev);
    }

    public void ChangeState(InputState to)
    {
        activeInputState.Exit(this);
        activeInputState = to;
        activeInputState.Enter(this);
    }

    internal void ChangeState(Node node)
    {
        throw new NotImplementedException();
    }

    // public void ChangeState(string to)
    // {
    //     this.ChangeState((InputState)GetNode<InputState>("InputStates").FindNode(to));
    // }

    public void ResetState()
    {
        this.ChangeState(GetNode<InputState>("InputStates/Main"));
    }

    private void Print(string message)
    {
        GD.Print(message);
        View.GetNode<MessageLog>("UILayer/MessageLog").AppendBbcode(
            $"\n * [color=#aa0000]{message}[/color]"
        );
    }
}
