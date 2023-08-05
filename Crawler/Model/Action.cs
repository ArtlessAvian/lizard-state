using Godot;
using System;
using System.Collections.Generic;

namespace LizardState.Engine
{
    /// <summary>
    /// The command pattern. Create action objects, use, then throw away.
    /// The action can also be "unreasonable."
    /// Actions should be serializable.
    /// </summary>
    public abstract class CrawlAction : Resource
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
        public CrawlAction SetTarget(AbsolutePosition target)
        {
            this.isRelative = false;
            this.targetInternal = new Vector2i(target.x, target.y);
            return this;
        }

        // hey its me ur brother
        public CrawlAction SetTargetRelative(Vector2i delta)
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

        public struct Line : Type
        {
        }
    }
}
