using System;
using Godot;
using LizardState.Engine;

public class AutoConfirmInputState : InputState
{
    private Crawler leakedCrawler;

    public override void Enter(Crawler crawler)
    {
        leakedCrawler = crawler;
    }

    public override void PollInput(Crawler crawler)
    {
        if (crawler.Model.GetPlayer().needsConfirmAction != null)
        {
            // GD.PrintS("Autoconfirming", crawler.Model.GetPlayer().needsConfirmAction.GetType().Name, "at time", crawler.Model.time);
            crawler.Model.SetPlayerAction(crawler.Model.GetPlayer().needsConfirmAction, false);
            crawler.notPlayerTurn = true;
            // stay in current state.
        }
        else
        {
            crawler.ResetState();
        }
    }

    // We still need this.
    // The parent calls Poll/HandleInput only when there's no animations playing.
    public override void _UnhandledInput(InputEvent ev)
    {
        if (leakedCrawler is Crawler crawler)
        {
            if (ev is InputEventKey key && key.IsPressed())
            {
                crawler.View.GetNode<MessageLog>("UILayer/MessageLog").AddMessage("Cancelling! (User Input)");
                crawler.ResetState();
                GetTree().SetInputAsHandled();
                return;
            }
            if (ev is InputEventMouseButton mouse && mouse.IsPressed())
            {
                crawler.View.GetNode<MessageLog>("UILayer/MessageLog").AddMessage("Cancelling! (User Input)");
                crawler.ResetState();
                GetTree().SetInputAsHandled();
                return;
            }
        }
    }

    public override void Exit(Crawler crawler)
    {
        leakedCrawler = null;
    }
}