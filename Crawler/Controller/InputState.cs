using Godot;

public abstract class InputState : Node
{
    protected static (string, (int, int))[] DIRECTIONS = {
        ("move_up", (0, -1)),
        ("move_down", (0, 1)),
        ("move_left", (-1, 0)),
        ("move_right", (1, 0)),
        ("move_upleft", (-1, -1)),
        ("move_upright", (1, -1)),
        ("move_downleft", (-1, 1)),
        ("move_downright", (1, 1)),
        ("move_wait", (0, 0))
    };

    // Input States /must/ be in SceneTree. Crawler/InputStates/../[InputState]
    protected Crawler GetCrawler()
    {
        Node parent = this.GetParent();
        while (!(parent is Crawler))
        {
            parent = parent.GetParent();
        }
        return parent as Crawler;
    }

    // Passing crawler is convenience.
    // (you can just traverse the tree for crawler, model, and view)
    public abstract void Enter(Crawler crawler);
    public abstract void HandleInput(Crawler crawler, InputEvent ev);    
    public abstract void Exit(Crawler crawler);
}