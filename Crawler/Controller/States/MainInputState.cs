using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using LizardState.Engine;

public class MainInputState : InputState
{
    Dictionary temp;

    Cursor cursor;
    Camera2D camera;

    public override void Enter(Crawler crawler)
    {
        cursor = crawler.GetNode<Cursor>("Cursor");
        camera = crawler.View.GetNode<Camera2D>("Camera2D");

        if (crawler.Model?.GetPlayer()?.needsConfirmAction is CrawlAction confirm)
        {
            foreach (string warning in confirm.GetWarnings(crawler.Model, crawler.Model.GetPlayer()))
            {
                crawler.View.GetNode<MessageLog>("UILayer/MessageLog").AddMessage(warning);
            }
        }
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
        if (ev.IsActionPressed("dump_model", false))
        {
            Model deepcopy = (Model)DeepCopyHelper.DeepCopy(crawler.Model);
            Error err = ResourceSaver.Save("res://dump.tres", deepcopy, ResourceSaver.SaverFlags.ReplaceSubresourcePaths);
            if (err == Error.Ok)
            {
                GD.Print("Dumped!");
            }
            else
            {
                GD.PrintErr("Failed to dump model: ", err);
            }
            return true;
        }

        if (ev is InputEventKey eventKey && eventKey.Pressed && !eventKey.IsEcho())
        {
            if (eventKey.Scancode == (int)KeyList.F10)
            {
                crawler.View.ModelSync();
                crawler.Model.SetPlayerAction(new FallAsleepAction()); //crawler, tuple.dir);
                crawler.notPlayerTurn = true;
                return true;
            }

            // this makes people very confused. also its a hack anyways.
            // if (eventKey.Scancode == (int)KeyList.F11)
            // {
            //     GetTree().ChangeScene("res://Crawler/Crawler.tscn");
            //     return true;
            // }
        }

        return false;
    }

    private bool TransitionInput(Crawler crawler, InputEvent ev)
    {
        for (int i = 0; i < 4; i++)
        {
            if (ev.IsActionPressed("skill_" + i))
            {
                // TODO: select ex version if available and shift held
                CrawlAction skill = (CrawlAction)crawler.Model.GetPlayer().species.abilities[i]?.Duplicate();
                ActionTargetInputState attackTargeting = this.GetNode<ActionTargetInputState>("AttackTargeting"); ;
                attackTargeting.action = skill;
                crawler.ChangeState(attackTargeting);
            }
        }

        if (ev.IsActionPressed("attack_action", false))
        {
            CrawlAction bumpAttack = crawler.Model.GetPlayer().species.bumpAttack;
            ActionTargetInputState attackTargeting = this.GetNode<ActionTargetInputState>("AttackTargeting"); ;
            attackTargeting.action = bumpAttack;
            crawler.ChangeState(attackTargeting);
            return true;
        }

        if (ev.IsActionPressed("menu_skills", false))
        {
            crawler.ChangeState(this.GetNode<InputState>("Skills"));
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

        foreach ((string name, Vector2i dir) tuple in DIRECTIONS)
        {
            if (ev.IsActionPressed(tuple.name, true))
            {
                Entity player = crawler.Model.GetPlayer();
                AbsolutePosition offset = player.position + tuple.dir;

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

        // TODO: DIRTY.
        if (ev is InputEventKey key && key.Scancode == (uint)KeyList.Enter)
        {
            if (crawler.Model.GetPlayer().needsConfirmAction != null)
            {
                crawler.Model.SetPlayerAction(crawler.Model.GetPlayer().needsConfirmAction, true);
                crawler.notPlayerTurn = true;
                return true;
            }
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
            cursor.targetPosition = new AbsolutePosition(
                Mathf.RoundToInt(mousePos.x / View.TILESIZE.x),
                Mathf.RoundToInt(mousePos.y / View.TILESIZE.y)
            );
            cursor.Show();

            // Draw a path between the player and the target.
        }

        if (ev is InputEventMouseButton evMouseButton)
        {
            if (evMouseButton.ButtonIndex == (int)ButtonList.Left && evMouseButton.IsPressed())
            {
                Vector2 mousePos = crawler.GetGlobalMousePosition();
                AbsolutePosition targetPosition = new AbsolutePosition(
                    Mathf.RoundToInt(mousePos.x / View.TILESIZE.x),
                    Mathf.RoundToInt(mousePos.y / View.TILESIZE.y)
                );

                Entity player = crawler.Model.GetPlayer();
                if (GridHelper.Distance(player.position, targetPosition) <= 1.5f)
                {
                    crawler.View.ModelSync();
                    crawler.Model.SetPlayerAction(new MoveOrAttackAction().SetTarget(targetPosition));
                    crawler.notPlayerTurn = true;
                    crawler.ResetState();
                    return true;
                }
                else
                {
                    crawler.View.ModelSync();
                    crawler.Model.SetPlayerAction(new GotoAction().SetTarget(targetPosition));
                    crawler.notPlayerTurn = true;
                    crawler.ResetState();
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
