using Godot;
using LizardState.Engine;

// To not do: replace with yield. Async/yield is equivalent to a state machine.
/// <summary>
/// InputStates are nodes in the scene tree.
/// As usual, nodes should be aware of their children, but not their parent.
/// 
/// 
/// Do not create any subclass at runtime!
/// If you do, something is has gone very wrong.
/// </summary>
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

    // All these are only called when there's no animation happening.
    // Passing crawler is convenience.
    // (you can just traverse the tree for crawler, model, and view)
    public virtual void Enter(Crawler crawler) { }
    public virtual void HandleInput(Crawler crawler, InputEvent ev) { }
    public virtual void PollInput(Crawler crawler) { }
    public virtual void Exit(Crawler crawler) { }
}