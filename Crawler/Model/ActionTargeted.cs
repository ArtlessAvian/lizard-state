public abstract class ActionTargeted : Action
{
    public (int, int) target = (0, 0);
    public abstract void Target(int x, int y);
    public abstract bool Do(ModelAPI api, Entity e);
}
