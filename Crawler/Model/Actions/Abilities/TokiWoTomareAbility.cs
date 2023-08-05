using Godot;
using System.Collections.Generic;
using LizardState.Engine;

public class TokiWoTomareAbility : CrawlAction
{
    // static Attack attack = new Attack(0.2f, 0.8f, 5, 0, 5);

    public TokiWoTomareAbility()
    {

    }

    public override bool Do(Model model, Entity e)
    {
        e.nextMove -= 4;
        e.energy -= 8;
        model.Debug("ZA WARUDO. TOKI WO TOMARE.");
        return true;
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }
}
