using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    ActionTargeted actionTargeting;

    public void OpenAbilities()
    {
        Popup menu = FindNode("Modals").GetNode<Popup>("AbilitiesMenu");
        // Setup the menu
        // Open the menu
        menu.Popup_();
        // wait for ability selected
    }

    public void AbilitySelected(int id)
    {
        (FindNode("AbilitiesMenu") as Popup).Hide();

        // Get player action
        Action action = new MoveAction((0, 0));
        // if aimed, shenanigans
        if (action is ActionTargeted temp)
        {
            // save for later! abuse of scope
            actionTargeting = temp;

            // activate and initialize cursor
            CursorMode cursorMode = FindNode("Modals").GetNode<CursorMode>("CursorMode");
            cursorMode.Enter(View.playerActor.targetPosition); // TODO: Decide getting position from model or viewmodel.
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
        actionTargeting.Target(x, y);
        model.DoPlayerAction(actionTargeting);
        notPlayerTurn = true;
    }
}
