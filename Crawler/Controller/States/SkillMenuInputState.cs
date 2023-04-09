using Godot;
using System;
using System.Collections.Generic;

public class SkillMenuInputState : InputState
{
    bool success;

    public override void Enter(Crawler crawler)
    {
        success = false;

        PopupMenu menu = crawler.FindNode("Modals").GetNode<PopupMenu>("AbilitiesMenu");

        menu.Clear();

        List<Action> abilities = crawler.Model.GetPlayer().species.abilities;
        for (int i = 0; i < abilities.Count; i++)
        {
            menu.AddItem(abilities[i].ResourceName, i);
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

        Action action = (Action)crawler.Model.GetPlayer().species.abilities[id].Duplicate();

        if (action.TargetingType is TargetingType.None)
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
