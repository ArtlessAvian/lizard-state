using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// The game, the "engine".
/// This handles a sequence of floors, ultimately resulting in a win or loss.
/// </summary>
public class Crawler : Node2D, InputStateMachine
{
    public View View
    {
        get { return GetNode<View>("View"); }
    }

    public Model Model
    {
        get { return GetNode<Model>("Model"); }
    }

    public InputState activeInputState;
    public bool notPlayerTurn = true;

    public override void _Ready()
    {
        // NoiseGenerator gen = new NoiseGenerator();
        // EditorGenerator gen = new EditorGenerator("res://Crawler/Generators/Maps/MVP-Scaled.tscn");
        // EditorGenerator gen = GD.Load<EditorGenerator>("res://Crawler/Playlist/Generator.tres");
        Playlist playlist = GD.Load("res://Crawler/Playlist/Failsafe.tres").Duplicate() as Playlist;
        playlist.FirstModel(Model);

        View.ConnectToModel(Model);

        activeInputState = GetNode<InputState>("InputStates/Main");
        activeInputState.Enter(this);

        // Model.CoolerApiEvent(-1, "Print", "[G]et the moss (green tiles) with the G key.");
        // Model.CoolerApiEvent(-1, "Print", "Then leave the cave (by stepping on a purple tile).");

        // if (GetViewport().Size.x >= 960 * 2)
        // {
        //     ProjectSettings.SetSetting("display/window/stretch/shrink", (int)GetViewport().Size.x / 960);
        // }
    }

    public override void _Process(float delta)
    {
        ulong start = OS.GetTicksMsec();
        this.RunModel(start);
    }

    // Treat as daemon.
    private void RunModel(ulong start)
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
            if (OS.GetTicksMsec() - start > 1000 / 144f)
            {
                GD.PrintErr("Timed out!");
                break;
            }
        }

        if (Model.done)
        {
            Model next = Model.playlist.NextModel(Model);
            RemoveChild(Model);
            AddChild(next);

            // TODO: Uncopypaste. Taken from MainInputState.cs
            View old = View;
            RemoveChild(old);
            old.QueueFree();

            PackedScene viewScene = GD.Load<PackedScene>("res://Crawler/View/View.tscn");
            View view = viewScene.Instance<View>();
            view.Name = "View";
            AddChild(view);

            view.ConnectToModel(next);
        }
    }

    public override void _UnhandledInput(InputEvent ev)
    {
        if (notPlayerTurn) { return; }
        if (View.eventQueue.Count > 0) { return; }
        activeInputState.HandleInput(this, ev);
    }

    public void ChangeState(InputState to)
    {
        activeInputState.Exit(this);
        activeInputState = to;
        activeInputState.Enter(this);
    }

    public void ResetState()
    {
        this.ChangeState(GetNode<InputState>("InputStates/Main"));
    }
}
