using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The command pattern. Create action objects, use, then throw away.
/// The action can also be "unreasonable."
/// Actions should be serializable.
/// </summary>
public abstract class Action : Resource
{
    // If !IsValid, no-ops. Caller should call MoveAction(0, 0)
    // to avoid getting stuck infinte looping.
    // If IsValid, tries to do the thing, even if unreasonable.
    // Returns IsValid. 
    public abstract bool Do(Model model, Entity e);

    // Checks if the action is leaves the model in a valid state.
    // Imagine this is a dry run, less expensive version of Do.
    // TODO: Create abstraction to hide Model details from UI and AI.
    public abstract bool IsValid(Model model, Entity e);

    // Gives a list of reasons this may be a bad move.
    // This is used by the UI and the AI.
    public virtual IEnumerable<string> GetWarnings(Model model, Entity e) { yield break; }

    // Targeting. Imagine a tagged union between relative and absolute, implemented poorly.
    [Export] private bool isRelative = true;
    [Export] private int targetInternalX;
    [Export] private int targetInternalY;
    private Vector2i targetInternal
    {
        get { return new Vector2i(targetInternalX, targetInternalY); }
        set { targetInternalX = value.x; targetInternalY = value.y; }
    }

    public AbsolutePosition GetTargetPos(AbsolutePosition origin)
    {
        if (isRelative)
        {
            return origin + targetInternal;
        }
        return new AbsolutePosition(targetInternal.x, targetInternal.y);
    }

    // its absolute
    public virtual Action SetTarget(AbsolutePosition target)
    {
        this.isRelative = false;
        this.targetInternal = new Vector2i(target.x, target.y);
        return this;
    }

    // hey its me ur brother
    public virtual Action SetTargetRelative(Vector2i delta)
    {
        this.isRelative = true;
        this.targetInternal = delta;
        return this;
    }

    /// For AI and UI use. Range is inclusive.
    [Obsolete]
    public virtual (int min, int max) Range => (0, 0);
    public virtual TargetingType.Type TargetingType => new TargetingType.None();
}

public static class TargetingType
{
    // TODO: Solidify function signatures.
    // Maybe create interface "model-like" to query tiles, entities?
    // Or somehow make immutable reference to the model. :P
    public interface Type
    {
        bool CheckValid(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction);
        IEnumerable<AbsolutePosition> GetAffectedTiles(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction);
        IEnumerable<AbsolutePosition> GetInfoTiles(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction);
        IEnumerable<AbsolutePosition> GetFullRange(AbsolutePosition sourcePos, Predicate<AbsolutePosition> blocksAction);
    }

    // TODO: Consider replacing with null? A "null" type is fine tbh.
    public struct None : Type
    {
        public bool CheckValid(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            return true;
        }

        public IEnumerable<AbsolutePosition> GetAffectedTiles(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            yield break;
        }

        public IEnumerable<AbsolutePosition> GetInfoTiles(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            yield break;
        }

        public IEnumerable<AbsolutePosition> GetFullRange(AbsolutePosition sourcePos, Predicate<AbsolutePosition> blocksAction)
        {
            yield break;
        }
    }

    public struct Cone : Type
    {
        public int radius;
        public float sectorDegrees;

        public bool CheckValid(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            return true;
        }

        IEnumerable<AbsolutePosition> Type.GetAffectedTiles(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            return VisibilityTrie.ConeOfView(sourcePos, blocksAction, radius, targetPos - sourcePos, sectorDegrees).Distinct();
        }

        public IEnumerable<AbsolutePosition> GetInfoTiles(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            yield break;
        }

        public IEnumerable<AbsolutePosition> GetFullRange(AbsolutePosition sourcePos, Predicate<AbsolutePosition> blocksAction)
        {
            return VisibilityTrie.FieldOfView(sourcePos, blocksAction, radius);
        }
    }

    public struct Smite : Type
    {
        public int range;
        public int splashRadius;

        public bool CheckValid(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            if (GridHelper.Distance(sourcePos, targetPos) > range) { return false; }
            if (!VisibilityTrie.AnyLineOfSight(sourcePos, targetPos, blocksAction)) { return false; }
            return true;
        }

        public IEnumerable<AbsolutePosition> GetAffectedTiles(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            // cursed return value.
            if (GridHelper.Distance(sourcePos, targetPos) > range) { return new AbsolutePosition[0]; }
            if (!VisibilityTrie.AnyLineOfSight(sourcePos, targetPos, blocksAction)) { return new AbsolutePosition[0]; }
            return VisibilityTrie.FieldOfView(targetPos, blocksAction, splashRadius).Distinct();
        }

        public IEnumerable<AbsolutePosition> GetInfoTiles(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            AbsolutePosition adjustedTarget = AdjustTarget(sourcePos, targetPos, blocksAction);

            int i = 0;
            foreach (AbsolutePosition pos in GridHelper.RayThrough(sourcePos, adjustedTarget))
            {
                yield return pos;
                if (blocksAction(pos)) { yield break; }
                if (i >= range) { yield break; }
                if (pos == targetPos) { yield break; }
                i++;
            }
        }

        public IEnumerable<AbsolutePosition> GetFullRange(AbsolutePosition sourcePos, Predicate<AbsolutePosition> blocksAction)
        {
            // Surprisingly performant(?)
            int localSplash = splashRadius;
            return VisibilityTrie.FieldOfView(sourcePos, blocksAction, range).Distinct().SelectMany(pos => VisibilityTrie.FieldOfView(pos, blocksAction, localSplash));
        }

        private AbsolutePosition AdjustTarget(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            if (GridHelper.LineBetween(sourcePos, targetPos).All(x => !blocksAction(x)))
            {
                return targetPos;
            }

            AbsolutePosition? newTarget = VisibilityTrie.SomeLineOfSight(sourcePos, targetPos, blocksAction);
            return newTarget ?? targetPos;
        }
    }

    // NOTE: Avoid using for dash like moves!
    // TODO: Reconcile both precise aiming and "stop at target".
    public struct Ray : Type
    {
        public int range;
        public bool stopAtTarget;

        public bool CheckValid(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            return true;
        }

        public IEnumerable<AbsolutePosition> GetAffectedTiles(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            AbsolutePosition adjustedTarget = AdjustTarget(sourcePos, targetPos, blocksAction);

            int i = 0;
            foreach (AbsolutePosition pos in GridHelper.RayThrough(sourcePos, adjustedTarget))
            {
                yield return pos;
                if (blocksAction(pos)) { yield break; }
                if (i >= range) { yield break; }
                if (stopAtTarget && pos == targetPos) { yield break; }
                i++;
            }
        }

        public IEnumerable<AbsolutePosition> GetInfoTiles(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            yield break;
        }

        public IEnumerable<AbsolutePosition> GetFullRange(AbsolutePosition sourcePos, Predicate<AbsolutePosition> blocksAction)
        {
            // TODO: Technically not true, but the function signature doesn't care for entities.
            return VisibilityTrie.FieldOfView(sourcePos, blocksAction, range);
        }

        private AbsolutePosition AdjustTarget(AbsolutePosition sourcePos, AbsolutePosition targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            if (GridHelper.LineBetween(sourcePos, targetPos).All(x => !blocksAction(x)))
            {
                return targetPos;
            }

            AbsolutePosition? newTarget = VisibilityTrie.SomeLineOfSight(sourcePos, targetPos, blocksAction);
            return newTarget ?? targetPos;
        }
    }
}
