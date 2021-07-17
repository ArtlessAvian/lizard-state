using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The command pattern. Create action objects, use, then throw away.
/// The action can also be "unreasonable."
/// </summary>
public abstract class Action
{
    // Do fails if IsValid is false. This /will/ waste a turn and might print a funny message.
    public abstract bool Do(ModelAPI api, Entity e);

    // Checks if the action is valid. This is mostly so the AI doesn't make an invalid move.
    // public abstract bool IsValid(ModelAPI api, Entity e);

    // Gives a list of reasons this may be a bad move.
    // public abstract string[] GetWarnings(ModelAPI api, Entity e);


    // Targeting
    private (int x, int y) targetInternal = (0, 0);
    private bool isRelative = true;

    public (int x, int y) GetTargetPos((int x, int y) origin)
    {
        if (isRelative)
        {
            return (targetInternal.x + origin.x, targetInternal.y + origin.y);
        }
        return targetInternal;
    }

    // its absolute
    public Action SetTarget((int x, int y) target)
    {
        this.isRelative = false;
        this.targetInternal = target;
        return this;
    }

    // hey its me ur brother
    public Action SetTargetRelative((int x, int y) delta)
    {
        this.isRelative = true;
        this.targetInternal = delta;
        return this;
    }

    // inclusive range
    public virtual (int min, int max) Range => (0, 0);
}
