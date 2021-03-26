using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    Action action;

    public void OpenAbilities()
    {
        Popup menu = FindNode("Modals").GetNode<Popup>("AbilitiesMenu");
        // Setup the menu
        // Open the menu
        menu.Popup_();
    }

    public void AbilitySelected(int id)
    {
        (FindNode("AbilitiesMenu") as Popup).Hide();

        // Get player action
        action = new MoveAction((0, 3));
        // if not aimed, run
        if (!action.IsAimed())
        {
            model.DoPlayerAction(action);
            notPlayerTurn = true;
        }
        else
        {
            CursorMode cursorMode = FindNode("Modals").GetNode<CursorMode>("CursorMode");
            
            // TODO: Decide if i want to get information from model or viewmodel.
            cursorMode.Enter(View.playerActor.targetPosition);
            cursorMode.Connect("Select", this, "AbilityTargeted");
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
        model.DoPlayerAction(action);
        notPlayerTurn = true;
    }
}
