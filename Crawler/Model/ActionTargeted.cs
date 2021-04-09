public abstract class ActionTargeted : Action
{
    protected (int x, int y) target = (0, 0);

    // its absolute
    public ActionTargeted Target((int x, int y) target)
    {
        this.target = target;
        return this;
    }

    // hey its me ur brother
    // public ActionTargeted TargetRelative((int x, int y) p, (int x, int y) d)
    // {
    //     this.target = (p.x + d.x, p.y + d.y);
    //     return this;
    // }

    public abstract bool Do(ModelAPI api, Entity e);
}
