using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class Crawler : Node2D
{
    public void _on_AbilitiesMenu_id_pressed(int id)
    {
        (FindNode("AbilitiesMenu") as Popup).Hide();
        model.DoPlayerAction(new MoveAction((0, 3)));
        notPlayerTurn = true;
    }
}
