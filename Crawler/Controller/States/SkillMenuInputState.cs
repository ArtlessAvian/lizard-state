using Godot;
using System;
using System.Collections.Generic;

public class SkillMenuInputState : InputState
{
    const int bigNumber = 100;

    bool success;

    public override void Enter(Crawler crawler)
    {
        success = false;

        PopupMenu menu = crawler.FindNode("Modals").GetNode<PopupMenu>("AbilitiesMenu");

        menu.Clear();
        menu.AddSeparator("Attacks");

        List<Action> attackData = crawler.Model.GetPlayer().species.attacks;
        for (int i = 0; i < attackData.Count; i++)
        {
            menu.AddItem(attackData[i].ResourceName, i);
        }

        menu.AddSeparator("Abilities");

        List<Action> abilities = crawler.Model.GetPlayer().species.abilities;
        for (int i = 0; i < abilities.Count; i++)
        {
            menu.AddItem(abilities[i].ResourceName, i + bigNumber);
        }

        menu.Popup_();
    }

    public override void HandleInput(Crawler crawler, InputEvent ev)
    {

    }

    public override void Exit(Crawler crawler)
    {
        (crawler.FindNode("AbilitiesMenu") as Popup).Hide();
    }

    public void _on_AbilitiesMenu_id_pressed(int id)
    {
        success = true;
        Crawler crawler = this.GetCrawler();

        Action action;
        if (id < bigNumber)
        {
            action = (Action)crawler.Model.GetPlayer().species.attacks[id].Duplicate();
        }
        else
        {
            action = (Action)crawler.Model.GetPlayer().species.abilities[id - bigNumber].Duplicate();
        }

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
        if (!success)
        {
            Crawler crawler = this.GetCrawler();
            crawler.ChangeState((InputState)this.GetParent());
        }
    }
}
