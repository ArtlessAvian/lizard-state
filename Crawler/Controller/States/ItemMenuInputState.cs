using Godot;
using System;
using System.Collections.Generic;
using LizardState.Engine;

public class ItemMenuInputState : InputState
{
    List<CrawlAction> items;

    bool success;

    public override void Enter(Crawler crawler)
    {
        success = false;

        PopupMenu menu = crawler.FindNode("Modals").GetNode<PopupMenu>("ItemsMenu");

        menu.Clear();
        menu.AddSeparator("Inventory");

        items = new List<CrawlAction>();

        int i = -1;
        foreach (InventoryItem item in crawler.Model.GetPlayer().inventory)
        {
            i++;
            string name = item.data.ResourceName;
            menu.AddItem($"{item.data.ResourceName} ({item.uses}/{item.data.maxUses})", i);
            menu.SetItemDisabled(menu.GetItemIndex(i), item.uses <= 0);

            CrawlAction action = item.BuildAction();
            items.Add(action);
        }
        if (crawler.Model.GetPlayer().inventory.Count == 0)
        {
            menu.AddItem("Nothing :(", 100);
            menu.SetItemDisabled(1, true);
        }

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

        CrawlAction action = items[id];
        GD.Print(id);

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

    public void _on_ItemsMenu_popup_hide()
    {
        if (!success)
        {
            Crawler crawler = this.GetCrawler();
            crawler.ChangeState((InputState)this.GetParent());
        }
    }
}
