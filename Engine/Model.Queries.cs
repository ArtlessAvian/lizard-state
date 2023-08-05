using System;
using System.Collections.Generic;
using Godot;

namespace LizardState.Engine
{
    public partial class Model
    {
        public CrawlerMap GetMap()
        {
            return this.map;
        }

        // Really, really slow.
        public T GetSystem<T>() where T : CrawlerSystem
        {
            foreach (Resource sys in systems)
            {
                if (sys is T systemCast) { return systemCast; }
            }
            return default(T);
        }

        public List<Entity> GetEntities()
        {
            return entities;
        }

        public List<FloorItem> GetFloorItems()
        {
            return floorItems;
        }

        public Entity GetEntity(int id)
        {
            return entities[id];
        }

        public Entity GetPlayer()
        {
            return GetEntity(0);
        }

        // There is one unique OK/STUN entity at every position.
        public Entity GetEntityAt(AbsolutePosition position)
        {
            foreach (Entity e in GetEntities())
            {
                // rip no tuple equality
                if (e.position.x == position.x && e.position.y == position.y)
                {
                    if (e.state == Entity.EntityState.OK || e.state == Entity.EntityState.STUN)
                    {
                        return e;
                    }
                }
            }
            return null;
        }

        public List<Entity> GetEntitiesInRadius(AbsolutePosition position, float radius)
        {
            List<Entity> inRadius = new List<Entity>();
            foreach (Entity e in GetEntities())
            {
                if (Distance(position, e.position) <= radius)
                {
                    if (e.state == Entity.EntityState.OK || e.state == Entity.EntityState.STUN)
                    {
                        inRadius.Add(e);
                    }
                }
            }
            return inRadius;
        }

        public List<Entity> GetEntitiesInLOS(AbsolutePosition position, float radius)
        {
            List<Entity> inSight = GetEntitiesInRadius(position, radius);

            for (int i = inSight.Count - 1; i >= 0; i--)
            {
                if (!VisibilityTrie.AnyLineOfSight(position, inSight[i].position, x => TileBlocksVision(x)))
                {
                    inSight.RemoveAt(i);
                }
            }
            return inSight;
        }

        public List<Entity> GetEntitiesInCone(AbsolutePosition position, float radius, Vector2i direction, float sectorDegrees)
        {
            // Not good code reuse. Copy-pasted from LOS.
            VisionSystem vision = GetSystem<VisionSystem>();
            // vision.trie.ExtendRadius(radius);

            List<Entity> inCone = GetEntitiesInRadius(position, radius);

            Predicate<AbsolutePosition> notInCone = target => !VisibilityTrie.TileInCone(position, target, direction, sectorDegrees);
            Predicate<AbsolutePosition> isBlocked = x => TileBlocksVision(x);
            Predicate<AbsolutePosition> isEither = target => notInCone(target) || isBlocked(target);

            for (int i = inCone.Count - 1; i >= 0; i--)
            {
                if (!VisibilityTrie.AnyLineOfSight(position, inCone[i].position, isEither))
                {
                    inCone.RemoveAt(i);
                }
            }
            return inCone;
        }

        public List<Entity> GetEntitiesInSight(int team)
        {
            if (team == 0)
            {
                VisionSystem visionSystem = GetSystem<VisionSystem>();
                List<Entity> entities = new List<Entity>();
                foreach (List<int> list in visionSystem.canSee.Values)
                {
                    foreach (int id in list)
                    {
                        Entity seen = GetEntity(id);
                        if (!entities.Contains(seen)) { entities.Add(seen); }
                    }
                }
                return entities;
            }
            else
            {
                // Just find check distance and line of sight directly.
                // TODO: Write it.
                return new List<Entity> { };
            }
        }

        // TODO: Disallow corner cutting?
        // Should be symmetric. f(x, y) = f(y, x).
        public bool CanWalkFromTo(AbsolutePosition position, AbsolutePosition position2)
        {
            return !map.TileIsWall(position2) &&
                    !map.TileIsWall(position);
        }

        public bool TileBlocksVision(AbsolutePosition position)
        {
            return map.TileIsWall(position);
        }

        public float Distance(AbsolutePosition pos, AbsolutePosition pos2)
        {
            return GridHelper.Distance(pos, pos2);
        }
    }
}
