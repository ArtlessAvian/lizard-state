using Godot;
using System;

public class AbilitiesMenu : PopupMenu
{
    public override void _Ready()
    {
        // Setup the character's attacks.
    }

    public void _on_id_pressed(int id)
    {
        GD.Print(id);
        Crawler crawler = GetNode<Crawler>("../../..");
        crawler.model.DoPlayerAction(crawler.eventQueue, new SpinAbility());
        crawler.notPlayerTurn = true;
    }
}
