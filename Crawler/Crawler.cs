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

    [Export]
    private Model model;
    public Model Model
    {
        get { return model; }
        set
        {
            model = value;
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
    }

    public InputState activeInputState;
    public bool notPlayerTurn = false;

    public Crawler() { }

    public override void _Ready()
    {
        // if (GetViewport().Size.x >= 960 * 2)
        // {
        //     ProjectSettings.SetSetting("display/window/stretch/shrink", (int)GetViewport().Size.x / 960);
        // }

        activeInputState = GetNode<InputState>("InputStates/Main");
        activeInputState.Enter(this);
    }

    public override void _Process(float delta)
    {
        if (Model == null) { return; }

        this.RunModel();
    }

    private void RunModel()
    {
        ulong start = OS.GetTicksMsec();
        int startModel = model.time;

        // Do nothing to prevent error spam.
        // (thrashing between model production, view consumption.)
        if (View.eventQueue.Count > 20) { return; }

        while (notPlayerTurn) // and not timed out
        {
            if (View.eventQueue.Count > 40 && !Input.IsKeyPressed((int)KeyList.F10))
            {
                // This reminds me of the minecraft message.
                GD.PrintErr("Too many queued events! (Model running far ahead of view?) Processing stopped to catch up.");
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
        }

        // Uncomment if not lag testing (which should be always)
        if (OS.GetTicksMsec() - start > 1000 / 144f)
        {
            GD.Print($"Turns {startModel}-{model.time} did not complete within frame. ({OS.GetTicksMsec() - start} ms)");
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
