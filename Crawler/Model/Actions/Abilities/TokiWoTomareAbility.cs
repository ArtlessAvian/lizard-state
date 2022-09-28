using Godot;
using Godot.Collections;

public class TokiWoTomareAbility : Action
{
    // static Attack attack = new Attack(0.2f, 0.8f, 5, 0, 5);

    public TokiWoTomareAbility()
    {

    }

    public override Dictionary Do(Model model, Entity e)
    {
        e.nextMove -= 4;
        e.energy -= 8;
        return CreateModelEvent(e.id, "Print", "ZA WARUDO. TOKI WO TOMARE.");
    }

    public override bool IsValid(Model model, Entity e)
    {
        return true;
    }
}
