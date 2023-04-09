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
    public interface Type
    {
        bool CheckValid((int x, int y) sourcePos, (int x, int y) targetPos, Predicate<AbsolutePosition> blocksAction);
        IEnumerable<AbsolutePosition> GetAffectedTiles((int x, int y) sourcePos, (int x, int y) targetPos, Predicate<AbsolutePosition> blocksAction);
        IEnumerable<AbsolutePosition> GetFullRange((int x, int y) sourcePos, Predicate<AbsolutePosition> blocksAction);
    }

    // TODO: Consider replacing with null? A "null" type is fine tbh.
    public struct None : Type
    {
        public bool CheckValid((int x, int y) sourcePos, (int x, int y) targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            return true;
        }

        public IEnumerable<AbsolutePosition> GetAffectedTiles((int x, int y) sourcePos, (int x, int y) targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            yield break;
        }

        public IEnumerable<AbsolutePosition> GetFullRange((int x, int y) sourcePos, Predicate<AbsolutePosition> blocksAction)
        {
            yield break;
        }
    }

    public struct Cone : Type
    {
        public int radius;
        public float sectorDegrees;

        public bool CheckValid((int x, int y) sourcePos, (int x, int y) targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            return true;
        }

        IEnumerable<AbsolutePosition> Type.GetAffectedTiles((int x, int y) sourcePos, (int x, int y) targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            return VisibilityTrie.ConeOfView(sourcePos, blocksAction, radius, (targetPos.x - sourcePos.x, targetPos.y - sourcePos.y), sectorDegrees);
        }

        public IEnumerable<AbsolutePosition> GetFullRange((int x, int y) sourcePos, Predicate<AbsolutePosition> blocksAction)
        {
            return VisibilityTrie.FieldOfView(sourcePos, blocksAction, radius);
        }
    }

    public struct Smite : Type
    {
        public int range;
        public int splashRadius;

        public bool CheckValid((int x, int y) sourcePos, (int x, int y) targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            if (GridHelper.Distance(sourcePos, targetPos) > range) { return false; }
            return true;
        }

        public IEnumerable<AbsolutePosition> GetAffectedTiles((int x, int y) sourcePos, (int x, int y) targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            if (GridHelper.Distance(sourcePos, targetPos) > range)
            {
                targetPos = GridHelper.StepTowards(sourcePos, targetPos, range);
            }
            return VisibilityTrie.FieldOfView(targetPos, blocksAction, splashRadius);
        }

        public IEnumerable<AbsolutePosition> GetFullRange((int x, int y) sourcePos, Predicate<AbsolutePosition> blocksAction)
        {
            // TODO: Make correct.
            return VisibilityTrie.FieldOfView(sourcePos, blocksAction, range + splashRadius);
        }
    }

    public struct Ray : Type
    {
        public int range;
        public bool stopAtTarget;

        public bool CheckValid((int x, int y) sourcePos, (int x, int y) targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            return true;
        }

        public IEnumerable<AbsolutePosition> GetAffectedTiles((int x, int y) sourcePos, (int x, int y) targetPos, Predicate<AbsolutePosition> blocksAction)
        {
            int i = 0;
            foreach ((int, int) pos in GridHelper.LineBetween(sourcePos, targetPos))
            {
                yield return pos;
                if (blocksAction(pos)) { yield break; }
                if (i >= range) { yield break; }
                i++;
            }
        }

        public IEnumerable<AbsolutePosition> GetFullRange((int x, int y) sourcePos, Predicate<AbsolutePosition> blocksAction)
        {
            // TODO: Technically not true, but the function signature doesn't care for entities.
            return VisibilityTrie.FieldOfView(sourcePos, blocksAction, range);
        }
    }
}
