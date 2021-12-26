using Godot;
using System;
using System.Collections.Generic;

public class AbilityInputState : InputState
{
    const int bigNumber = 100;

    List<ReachAttackData> attackData;
    List<string> abilities;

    bool success;

    public override void Enter(Crawler crawler)
    {
        success = false;

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
        for (int i = 0; i < abilities.Count; i++)
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
        success = true;
        // GD.Print("eeeeeeeeeeee first");
        Crawler crawler = this.GetCrawler();

        Action action;
        if (id < bigNumber)
        {
            action = new ReachAttackAction(null);
            // action = new AttackAction();
            // action = new AttackAction(attackData[id]);
        }
        else
        {
            action = (Action)Activator.CreateInstance(Type.GetType(abilities[id - bigNumber]));
        }

        if (action.Range.max == 0)
        {
            crawler.Model.SetPlayerAction(action);
            crawler.notPlayerTurn = true;
            crawler.ResetState();
        }
        else
        {
            AbilityTargetInputState to = this.GetNode<AbilityTargetInputState>("Targeting");
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
        // GD.Print("eeeeeeeeeeee second");
    }
}
