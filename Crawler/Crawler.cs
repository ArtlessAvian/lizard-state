using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// The model, view, and controller, composed in Godot.
/// Swapping out the model cleanly swaps out the view and resets the controller.
/// </summary>
public class Crawler : Node2D, InputStateMachine
{
    [Signal]
    public delegate void Done();

    public View View
    {
        get { return GetNode<View>("View"); }
    }

    public Model Model;

    public InputState activeInputState;
    public bool notPlayerTurn = false;

    public Crawler()
    {
        Model = (Model)GD.Load("res://Crawler/Model/Model.tres");
    }

    public void InitializeForReal(Model model)
    {
        Model = model;
        // Swap out the view too.
        {
            View old = GetNode<View>("View");
            RemoveChild(old);
            old.QueueFree();

            PackedScene viewScene = GD.Load<PackedScene>("res://Crawler/View/View.tscn");
            View view = viewScene.Instance<View>();
            view.Name = "View";
            AddChild(view);
            MoveChild(view, 1);

            view.ConnectToModel(Model);
        }
        activeInputState = GetNode<InputState>("InputStates/Main");
        activeInputState.Enter(this);
        notPlayerTurn = true;
    }

    public override void _Ready()
    {
        // View.ConnectToModel(Model);
        activeInputState = GetNode<InputState>("InputStates/Main");
        activeInputState.Enter(this);

        // move this to parent.
        // NoiseGenerator gen = new NoiseGenerator();
        // EditorGenerator gen = new EditorGenerator("res://Crawler/Generators/Maps/MVP-Scaled.tscn");
        // EditorGenerator gen = GD.Load<EditorGenerator>("res://Crawler/Playlist/Generator.tres");
        // ExplorePlaylist playlist = GD.Load("res://GameModes/Story/Explore/Playlist/Failsafe.tres").Duplicate() as ExplorePlaylist;
        // InitializeForReal(playlist.GetCurrentModel());

        // Model.CoolerApiEvent(-1, "Print", "[G]et the moss (green tiles) with the G key.");
        // Model.CoolerApiEvent(-1, "Print", "Then leave the cave (by stepping on a purple tile).");

        // if (GetViewport().Size.x >= 960 * 2)
        // {
        //     ProjectSettings.SetSetting("display/window/stretch/shrink", (int)GetViewport().Size.x / 960);
        // }

        // PlanarGenerator gen = new PlanarGenerator();
        // gen.GenerateEmbedding();
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
            if (View.eventQueue.Count > 40)
            {
                GD.PrintErr("Too many queued events! Model running far ahead of view?");
                break;
            }

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

        if (Model.done && View.done && !GetNode<AnimationPlayer>("Fader/AnimationPlayer").IsPlaying())
        {
            // Hack-ish.
            GetNode<AnimationPlayer>("Fader/AnimationPlayer").Play("FadeOut");
            SceneTreeTimer sceneTreeTimer = GetTree().CreateTimer(1, true);
            sceneTreeTimer.Connect("timeout", this, "NextFloor");
        }
    }

    private void NextFloor()
    {
        GetNode<AnimationPlayer>("Fader/AnimationPlayer").Play("FadeIn");
        EmitSignal("Done");
    }

    public override void _UnhandledInput(InputEvent ev)
    {
        if (notPlayerTurn) { return; }
        if (View.eventQueue.Count > 0) { return; }
        if (View.done) { return; }
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
