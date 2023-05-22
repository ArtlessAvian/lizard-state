using Godot;

// TODO: consider replacing with yield to simulate blocking code.
// Trading one hellscape for another.
// in c# that would be "await ToSignal(signal)"
public abstract class InputState : Node
{
    protected static (string, Vector2i)[] DIRECTIONS = {
        ("move_up", new Vector2i(0, -1)),
        ("move_down", new Vector2i(0, 1)),
        ("move_left", new Vector2i(-1, 0)),
        ("move_right", new Vector2i(1, 0)),
        ("move_upleft", new Vector2i(-1, -1)),
        ("move_upright", new Vector2i(1, -1)),
        ("move_downleft", new Vector2i(-1, 1)),
        ("move_downright", new Vector2i(1, 1)),
        ("move_wait", new Vector2i(0, 0))
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