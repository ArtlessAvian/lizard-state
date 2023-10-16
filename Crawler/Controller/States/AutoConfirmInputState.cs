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

    public override void HandleInput(Crawler crawler, InputEvent ev)
    {
        // Do nothing.
    }

    public override void _Process(float delta)
    {
        if (leakedCrawler is Crawler crawler)
        {
            // This isn't Node.Input. The parent calls this only when there's no animations playing.
            if (Input.IsKeyPressed((int)KeyList.Escape))
            {
                crawler.View.GetNode<MessageLog>("UILayer/MessageLog").AddMessage("Cancelling! (User Input)");
                crawler.ResetState();
                return;
            }

            if (crawler.GetNode<View>("View").IsQueueClear())
            {
                if (crawler.Model.GetPlayer().needsConfirmAction != null)
                {
                    GD.PrintS("Autoconfirming", crawler.Model.GetPlayer().needsConfirmAction.GetType().Name, "at time", crawler.Model.time);
                    crawler.Model.SetPlayerAction(crawler.Model.GetPlayer().needsConfirmAction, false);
                    crawler.notPlayerTurn = true;
                    // stay in current state.
                }
                else
                {
                    crawler.ResetState();
                }
            }
        }
    }

    public override void Exit(Crawler crawler)
    {
        leakedCrawler = null;
    }
}