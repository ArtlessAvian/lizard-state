using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    ActionTargeted actionTargeting;

    public void OpenAbilities()
    {
        PopupMenu menu = FindNode("Modals").GetNode<PopupMenu>("AbilitiesMenu");
        // Setup the menu
        {
            menu.Clear();
            menu.AddSeparator("Abilities");
            
            int id = 0;
            foreach (Action a in model.GetPlayer().abilities)
            {
                menu.AddItem(a.GetType().Name, id);
                id++;
            }
        }

        // Open the menu
        menu.Popup_();
        // wait for ability selected
    }

    public void AbilitySelected(int id)
    {
        (FindNode("AbilitiesMenu") as Popup).Hide();

        // Get player action
        Action action = model.GetPlayer().abilities[id];

        // if aimed, shenanigans
        if (action is ActionTargeted temp)
        {
            // save for later! abuse of scope
            actionTargeting = temp;

            // activate and initialize cursor
            CursorMode cursorMode = FindNode("Modals").GetNode<CursorMode>("CursorMode");
            cursorMode.Enter(model.GetPlayer().position);
            cursorMode.Connect("Select", this, "AbilityTargeted");
            return; // wait for ability targeted
        }
        else // just run it directly
        {
            model.DoPlayerAction(action);
            notPlayerTurn = true;
        }
    }

    public void AbilityTargeted(int x, int y)
    {
        // Check if valid
        // Disconnect
        CursorMode cursorMode = FindNode("Modals").GetNode<CursorMode>("CursorMode");
        cursorMode.Disconnect("Select", this, "AbilityTargeted");
        cursorMode.Exit();

        // Do the move
        actionTargeting.Target((x, y));
        model.DoPlayerAction(actionTargeting);
        notPlayerTurn = true;
    }
}
