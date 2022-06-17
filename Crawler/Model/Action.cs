using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The command pattern. Create action objects, use, then throw away.
/// The action can also be "unreasonable."
/// Actions should be serializable.
/// </summary>
public abstract class Action : Resource
{
    // If !IsValid, wastes some time and sends error event.
    // Otherwise, tries to do the thing, even if unreasonable. 
    public abstract bool Do(Model model, Entity e);

    // Checks if the action is valid.
    // This is used by the UI and the AI.
    public abstract bool IsValid(Model model, Entity e);

    // Gives a list of reasons this may be a bad move.
    // public abstract string[] GetWarnings(ModelAPI api, Entity e);

    // Targeting
    [Export] public int targetInternalX;
    [Export] public int targetInternalY;
    public (int x, int y) targetInternal
    {
        get { return (targetInternalX, targetInternalY); }
        set { targetInternalX = value.x; targetInternalY = value.y; }
    }
    [Export]
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
    public virtual Action SetTarget((int x, int y) target)
    {
        this.isRelative = false;
        this.targetInternal = target;
        return this;
    }

    // hey its me ur brother
    public virtual Action SetTargetRelative((int x, int y) delta)
    {
        this.isRelative = true;
        this.targetInternal = delta;
        return this;
    }

    /// For AI and UI use. Range is inclusive.
    public virtual (int min, int max) Range => (0, 0);

    public Godot.Collections.Dictionary SaveToDictionary()
    {
        Godot.Collections.Dictionary dict = new Godot.Collections.Dictionary();
        dict["targetInternalX"] = targetInternal.x;
        dict["targetInternalY"] = targetInternal.y;
        dict["isRelative"] = isRelative;

        return dict;
    }
}
