using Godot;
using System;
using System.Collections.Generic;

public class ItemInputState : InputState
{
    List<AttackData> attackData;
    List<string> abilities;

    bool success;

    public override void Enter(Crawler crawler)
    {
        success = false;

        PopupMenu menu = crawler.FindNode("Modals").GetNode<PopupMenu>("ItemsMenu");

        menu.Clear();

        string name = crawler.Model.GetPlayer().inventory.GetName();
        menu.AddItem(name, 0);

        menu.Popup_();
    }

    public override void HandleInput(Crawler crawler, InputEvent ev)
    {

    }

    public override void Exit(Crawler crawler)
    {
        (crawler.FindNode("ItemsMenu") as Popup).Hide();
    }

    public void _on_ItemsMenu_id_pressed(int id)
    {
        success = true;
        Crawler crawler = this.GetCrawler();

        Action action = new UseItemAction(crawler.Model.GetPlayer(), 3458799);

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

    public void _on_ItemsMenu_popup_hide()
    {
        if (!success)
        {
            Crawler crawler = this.GetCrawler();
            crawler.ChangeState((InputState)this.GetParent());
        }
        // GD.Print("eeeeeeeeeeee second");
    }
}
