using Godot;
using Godot.Collections;

public class ExitAction : Action
{
    public override Dictionary Do(Model model, Entity e)
    {
        if (!IsValid(model, e))
        {
            return null;
        }

        (int x, int y) = GetTargetPos(e.position);

        if (model.GetMap().GetCell(x, y) == 5)
        {
            e.nextMove = -1;
            model.done = true;
            return CreateModelEvent(0, "Exit");
        }
        return null;
    }

    public override bool IsValid(Model model, Entity e)
    {
        (int x, int y) = GetTargetPos(e.position);
        if (model.GetMap().GetCell(x, y) == 5)
        {
            return true;
        }
        return false;
    }
}
