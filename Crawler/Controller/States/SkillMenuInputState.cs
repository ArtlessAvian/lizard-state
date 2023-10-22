using Godot;
using System;
using System.Collections.Generic;
using LizardState.Engine;

public class SkillMenuInputState : InputState
{
    private bool cancelled;

    public override void Enter(Crawler crawler)
    {
        cancelled = true;
        List<CrawlAction> abilities = crawler.Model.GetPlayer().species.abilities;

        PopupMenu menu = crawler.FindNode("Modals").GetNode<PopupMenu>("AbilitiesMenu");
        menu.Clear();
        for (int i = 0; i < abilities.Count; i++)
        {
            menu.AddItem(abilities[i].ResourceName, i);
        }
        menu.Popup_();
    }

    public override void Exit(Crawler crawler)
    {
        (crawler.FindNode("AbilitiesMenu") as Popup).Hide();
    }

    public void _on_AbilitiesMenu_id_pressed(int id)
    {
        cancelled = false;
        Crawler crawler = this.GetCrawler();

        CrawlAction action = (CrawlAction)crawler.Model.GetPlayer().species.abilities[id].Duplicate();

        if (action.Range.max == 0)
        {
            crawler.View.ModelSync();
            crawler.Model.SetPlayerAction(action);
            crawler.notPlayerTurn = true;
            crawler.ResetState();
        }
        else
        {
            ActionTargetInputState to = this.GetNode<ActionTargetInputState>("Targeting");
            to.action = action;
            crawler.ChangeState(to);
            return;
        }
    }

    public void _on_AbilitiesMenu_popup_hide()
    {
        if (cancelled)
        {
            Crawler crawler = this.GetCrawler();
            crawler.ChangeState((InputState)this.GetParent());
        }
    }
}
