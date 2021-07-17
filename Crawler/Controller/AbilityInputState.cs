using Godot;
using System;
using System.Collections.Generic;

public class AbilityInputState : InputState
{
    const int bigNumber = 100;

    List<AttackData> attackData;
    List<string> abilities;

    public override void Enter(Crawler crawler)
    {
        PopupMenu menu = crawler.FindNode("Modals").GetNode<PopupMenu>("AbilitiesMenu");

        menu.Clear();
        menu.AddSeparator("Attacks");
        
        attackData = crawler.Model.GetPlayer().species.attacks;
        for (int i = 0; i < attackData.Count; i++)
        {            
            menu.AddItem(attackData[i].ResourceName, i);
        }

        menu.AddSeparator("Abilities");
        
        abilities = crawler.Model.GetPlayer().species.abilities;
        for (int i = 0; i < attackData.Count; i++)
        {            
            menu.AddItem(abilities[i], i + bigNumber);
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
        Crawler crawler = this.GetCrawler();

        Action action;
        if (id < bigNumber)
        {
            action = new AttackAction(id);
        }
        else
        {
            action = (Action)Activator.CreateInstance(Type.GetType(abilities[id - bigNumber]));
        }

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
            crawler.Model.SetPlayerAction(action);
            crawler.notPlayerTurn = true;
            crawler.ResetState();
        }

    }
}
