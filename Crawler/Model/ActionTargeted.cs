
/// <summary>
/// Action, with stuff for convenience.
/// </summary>
public abstract class ActionTargeted : Action
{
    private (int x, int y) targetInternal = (0, 0);
    private bool isRelative = true;

    protected (int x, int y) GetTargetPos((int x, int y) origin)
    {
        if (isRelative)
        {
            return (targetInternal.x + origin.x, targetInternal.y + origin.y);
        }
        return targetInternal;
    }

    // its absolute
    public ActionTargeted SetTarget((int x, int y) target)
    {
        this.isRelative = false;
        this.targetInternal = target;
        return this;
    }

    // hey its me ur brother
    public ActionTargeted SetTargetRelative((int x, int y) delta)
    {
        this.isRelative = true;
        this.targetInternal = delta;
        return this;
    }

    public abstract bool Do(ModelAPI api, Entity e);
}
