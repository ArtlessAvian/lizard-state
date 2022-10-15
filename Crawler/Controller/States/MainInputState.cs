using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public class MainInputState : InputState
{
    Dictionary temp;

    Cursor cursor;
    Camera2D camera;

    public override void Enter(Crawler crawler)
    {
        cursor = crawler.GetNode<Cursor>("Cursor");
        camera = crawler.View.GetNode<Camera2D>("Camera2D");
    }

    public override void HandleInput(Crawler crawler, InputEvent ev)
    {
        if (this.DebugInput(crawler, ev))
        { GetTree().SetInputAsHandled(); return; }

        if (this.TransitionInput(crawler, ev))
        { GetTree().SetInputAsHandled(); return; }

        if (this.LogicInput(crawler, ev))
        { GetTree().SetInputAsHandled(); return; }
    }

    public bool DebugInput(Crawler crawler, InputEvent ev)
    {
        // Only saves the model. Not intended as a long term solution.
        if (ev.IsActionPressed("quicksave", false))
        {
            ResourceSaver.Save("res://dump.tres", crawler.Model);
            return true;
        }

        if (ev.IsActionPressed("quickload", false))
        {
            Model model = (Model)GD.Load("res://dump.tres").Duplicate();
            crawler.Model = model;

            return true;
        }

        if (ev is InputEventKey eventKey && eventKey.Pressed && !eventKey.IsEcho())
        {
            if (eventKey.Scancode == (int)KeyList.F1)
            {
                crawler.View.impatientMode = !crawler.View.impatientMode;
                string thing = (crawler.View.impatientMode ? "on" : "off");

                crawler.View.GetNode<RichTextLabel>("UILayer/MessageLog").AppendBbcode($"\n * Impatient mode {thing}!");
                return true;
            }

            // this makes people very confused. also its a hack anyways.
            // if (eventKey.Scancode == (int)KeyList.F11)
            // {
            //     GetTree().ChangeScene("res://Crawler/Crawler.tscn");
            //     return true;
            // }

            if (eventKey.Scancode == (int)KeyList.Quoteleft)
            {
                Control debugLog = crawler.View.GetNode<Control>("UILayer/DebugLog");
                debugLog.Visible = !debugLog.Visible;
                return true;
            }
        }

        return false;
    }

    private bool TransitionInput(Crawler crawler, InputEvent ev)
    {
        if (ev.IsActionPressed("menu_abilities", false))
        {
            crawler.ChangeState(this.GetNode<InputState>("Ability"));
            return true;
        }

        if (ev.IsActionPressed("menu_items", false))
        {
            crawler.ChangeState(this.GetNode<InputState>("Item"));
            return true;
        }

        if (ev.IsActionPressed("look"))
        {
            crawler.ChangeState(this.GetNode<InputState>("Look"));
            return true;
        }

        return false;
    }

    private bool LogicInput(Crawler crawler, InputEvent ev)
    {
        if (Input.IsKeyPressed((int)KeyList.Minus))
        {
            camera.Zoom = Vector2.One;
        }
        if (Input.IsKeyPressed((int)KeyList.Equal))
        {
            camera.Zoom = Vector2.One / 2;
        }

        foreach ((string name, (int x, int y) dir) tuple in DIRECTIONS)
        {
            if (ev.IsActionPressed(tuple.name, true))
            {
                Entity player = crawler.Model.GetPlayer();
                (int x, int y) offset = (player.position.x + tuple.dir.x, player.position.y + tuple.dir.y);

                if (Input.IsKeyPressed((int)KeyList.Control))
                {
                    crawler.View.ModelSync();
                    crawler.Model.SetPlayerAction(new RunAction().SetTarget(offset)); //crawler, tuple.dir);
                    crawler.notPlayerTurn = true;
                    cursor.Hide();
                    return true;
                }
                else
                {
                    crawler.View.ModelSync();
                    crawler.Model.SetPlayerAction(new MoveOrAttackAction().SetTarget(offset)); //crawler, tuple.dir);
                    crawler.notPlayerTurn = true;
                    cursor.Hide();
                    return true;
                }
            }
        }

        if (ev.IsActionPressed("get_action"))
        {
            crawler.View.ModelSync();
            crawler.Model.SetPlayerAction(new GetAction());
            crawler.notPlayerTurn = true;
            return true;
        }

        // if (ev.IsActionPressed("exit_action"))
        // {
        //     crawler.Model.SetPlayerAction(new ExitAction());
        //     crawler.notPlayerTurn = true;
        //     return true;
        // }

        if (ev is InputEventMouse evMouse)
        {
            // is it possible to get it from the thing instead?
            Vector2 mousePos = crawler.GetGlobalMousePosition();
            // Temporary.
            cursor.targetPosition.x = Mathf.RoundToInt(mousePos.x / View.TILESIZE.x);
            cursor.targetPosition.y = Mathf.RoundToInt(mousePos.y / View.TILESIZE.y);
            cursor.Show();

            // Draw a path between the player and the target.
        }

        if (ev is InputEventMouseButton evMouseButton)
        {
            if (evMouseButton.ButtonIndex == (int)ButtonList.Left && evMouseButton.IsPressed())
            {
                Vector2 mousePos = crawler.GetGlobalMousePosition();
                (int x, int y) targetPosition;
                targetPosition.x = Mathf.RoundToInt(mousePos.x / View.TILESIZE.x);
                targetPosition.y = Mathf.RoundToInt(mousePos.y / View.TILESIZE.y);

                Entity player = crawler.Model.GetPlayer();
                if (GridHelper.Distance(player.position, targetPosition) <= 1.5f)
                {
                    crawler.View.ModelSync();
                    crawler.Model.SetPlayerAction(new MoveOrAttackAction().SetTarget(targetPosition));
                    crawler.notPlayerTurn = true;
                    return true;
                }
                else
                {
                    crawler.View.ModelSync();
                    crawler.Model.SetPlayerAction(new GotoAction().SetTarget(targetPosition));
                    crawler.notPlayerTurn = true;
                    return true;
                }
            }
        }

        return false;
    }

    // private bool MoveOrAttack(Crawler crawler, (int x, int y) direction)
    // {
    //     // Entity entityAt = crawler.Model.GetEntityAt(offset);

    //     // if (entityAt != null && entityAt.team != player.team)
    //     // {
    //     //     return crawler.Model.DoPlayerAction(new AttackAction(player.species.bumpAttack).Target(offset));
    //     // }
    //     // return crawler.Model.DoPlayerAction(new MoveAction().Target(offset));
    // }

    public override void Exit(Crawler crawler)
    {
        cursor.Hide();
    }
}
