using Godot;
using System;
using System.Collections.Generic;

public class AbilityInputState : InputState
{
    List<Action> abilities;

    public override void Enter(Crawler crawler)
    {
        PopupMenu menu = crawler.FindNode("Modals").GetNode<PopupMenu>("AbilitiesMenu");

        menu.Clear();
        menu.AddSeparator("Abilities");
        
        int id = 0;
        abilities = crawler.Model.GetPlayer().abilities;
        foreach (Action a in abilities)
        {
            menu.AddItem(a.GetType().Name, id);
            id++;
        }

        menu.Popup_();
    }

    public override void Input(Crawler crawler, InputEvent ev)
    {
        throw new NotImplementedException();
    }

    public override void Exit(Crawler crawler)
    {
        (crawler.FindNode("AbilitiesMenu") as Popup).Hide();
    }

    public void _on_AbilitiesMenu_id_pressed(int id)
    {
        Crawler crawler = this.GetCrawler();

        // Get player action
        Action action = abilities[id];

        // if aimed, shenanigans
        if (action is ActionTargeted temp)
        {
            // save for later! abuse of scope
            AbilityTargetInputState to = this.GetNode<AbilityTargetInputState>("Targeting");
            to.action = temp;
            crawler.ChangeState(to);
            return;
        }
        else // just run it directly
        {
            crawler.Model.DoPlayerAction(action);
            crawler.notPlayerTurn = true;
            crawler.ResetState();
        }

    }
}
