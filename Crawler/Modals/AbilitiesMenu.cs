using Godot;
using System;

public class AbilitiesMenu : PopupMenu
{
    AttackData throww;

    public override void _Ready()
    {
        // Setup the character's attacks.
        throww = GD.Load<AttackData>("res://Crawler/Model/Attacks/BasicThrow.tres"); 
    }

    public void _on_id_pressed(int id)
    {
        GD.Print(id);
        View view = GetNode<View>("../../..");
        view.model.DoPlayerAction(new AttackAction((0, 1), throww));
        view.notPlayerTurn = true;
        
        this.Hide();
    }
}
