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
    // If !IsValid, no-ops. Caller should waste some model time
    // to avoid AI getting stuck infinte looping.
    // If IsValid, tries to do the thing, even if unreasonable.
    // Returns IsValid. 
    public abstract bool Do(Model model, Entity e);

    // Checks if the action is valid, leaving the model in a valid state.
    // Imagine this is a dry run, less expensive version of Do.
    // This is used by the UI and the AI.
    // TODO: Create abstraction to hide Model details from UI and AI.
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
    public virtual TargetingType.Type TargetingType => new TargetingType.Smite(0);
}

public static class TargetingType
{
    public interface Type { }

    // TODO: add more params.
    public struct Cone : Type
    {
        public float sectorDegrees;
        public Cone(float sectorDegrees)
        {
            this.sectorDegrees = sectorDegrees;
        }
    }

    public struct Smite : Type
    {
        public float radius;
        public Smite(float radius)
        {
            this.radius = radius;
        }
    }

    public struct Ray : Type
    {
        public bool pierces;
        public bool stopAtTarget;
        public Ray(bool pierces, bool stopAtTarget)
        {
            this.pierces = pierces;
            this.stopAtTarget = stopAtTarget;
        }
    }
}
