
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LizardState.Engine
{
    // See http://www.roguebasin.com/index.php?title=Pre-Computed_Visibility_Tries
    //  Don't worry about static. Treat as big constant!
    // TODO: Revise the staticness of it
    public class VisibilityTrie
    {
        public class TrieNode
        {
            public int x = 0;
            public int y = 0;
            public TrieNode straight = null;
            public TrieNode diag = null;
            public TrieNode up = null;
            public TrieNode parent = null;

            public TrieNode(int x, int y, TrieNode parent = null)
            {
                this.x = x;
                this.y = y;
                this.parent = parent;
            }
        }

        private static TrieNode origin;
        private static int currentRadius = 0;
        private static Dictionary<(int x, int y), HashSet<TrieNode>> reverse;

        static VisibilityTrie()
        {
            origin = new TrieNode(0, 0);
            reverse = new Dictionary<(int x, int y), HashSet<TrieNode>>();
            AddToReverse(origin);

            ExtendRadius(20);
        }

        private static void ExtendRadius(int radius)
        {
            if (currentRadius >= radius) { return; }
            currentRadius = radius;

            // For each unique ray in the octant passing through a cell,
            foreach ((int rise, int run) in GridHelper.ListRationals((int)radius + 1))
            {
                // Add the tiles the ray hits to the trie.
                TrieNode current = origin;
                // TODO: Replace with Vector2i.
                foreach (AbsolutePosition tile in GridHelper.RayThrough(new AbsolutePosition(0, 0), new AbsolutePosition(run, rise)))
                {
                    if (tile.x == 0) { continue; } // skip the first one.
                    if (tile.x > radius) { break; } // finish up
                    if (tile.y == current.y)
                    {
                        if (current.straight is null)
                        {
                            current.straight = new TrieNode(tile.x, tile.y, current);
                            AddToReverse(current.straight);
                        }
                        current = current.straight;
                    }
                    else if (tile.x == current.x)
                    {
                        if (current.up is null)
                        {
                            current.up = new TrieNode(tile.x, tile.y, current);
                            AddToReverse(current.up);
                        }
                        current = current.up;
                    }
                    else
                    {
                        if (current.diag is null)
                        {
                            current.diag = new TrieNode(tile.x, tile.y, current);
                            AddToReverse(current.diag);
                        }
                        current = current.diag;
                    }
                }
            }
        }

        private static void AddToReverse(TrieNode node)
        {
            (int x, int y) pos = (node.x, node.y);
            if (!reverse.ContainsKey(pos))
            {
                reverse.Add(pos, new HashSet<TrieNode>());
            }
            reverse[pos].Add(node);
        }

        // The reall stuff.

        /// May yield duplicates! If critical, use a set.
        public static IEnumerable<Vector2i> FieldOfViewRelative(Predicate<Vector2i> isBlocked, float radius)
        {
            for (int octant = 0; octant < 8; octant++)
            {
                List<TrieNode> stack = new List<TrieNode> { origin };
                while (stack.Count > 0)
                {
                    TrieNode current = stack[stack.Count - 1]; // TIL the hat operator ^0
                    stack.RemoveAt(stack.Count - 1);

                    if (current == null || GridHelper.Distance(current.x, current.y) > radius)
                    {
                        continue;
                    }

                    Vector2i relativePos = GridHelper.DeOctantify(current.x, current.y, octant);
                    yield return relativePos;
                    if (!isBlocked(relativePos))
                    {
                        stack.Add(current.straight);
                        stack.Add(current.diag);
                        stack.Add(current.up);
                    }
                }
            }
        }

        public static IEnumerable<Vector2i> ConeOfViewRelative(Predicate<Vector2i> isBlocked, float radius, Vector2i direction, float sectorDegrees)
        {
            // List all tiles on the square with given radius.
            HashSet<Vector2i> allTargets = new HashSet<Vector2i>();
            for (int octant = 0; octant < 8; octant++)
            {
                for (int dy = 0; dy <= radius; dy++)
                {
                    Vector2i targetRelative = GridHelper.DeOctantify((int)radius, dy, octant);
                    allTargets.Add(targetRelative);
                }
            }

            // Find the top tiles along the arc.
            int arcLength = (int)Math.Ceiling(radius * sectorDegrees * SquareSectorHelper.FULL_REVOLUTION / 360f);
            // attackers can have a little symmetry as a treat
            if (arcLength % 2 == 0) { arcLength++; }

            var closestTargets = allTargets.OrderBy(target => SquareSectorHelper.AngleBetween(target.x, target.y, direction.x, direction.y))
                                           .Take(arcLength);

            foreach (Vector2i targetRelative in closestTargets)
            {
                (int _, int dy, int octant) = GridHelper.Octantify(targetRelative);
                foreach (TrieNode origin in reverse[((int)radius, dy)])
                {
                    List<Vector2i> pending = new List<Vector2i>();
                    TrieNode current = origin;
                    while (current.x != 0 || current.y != 0)
                    {
                        Vector2i currentRelative = GridHelper.DeOctantify(current.x, current.y, octant);
                        if (isBlocked(currentRelative))
                        {
                            pending.Clear();
                        }
                        pending.Add(currentRelative);
                        current = current.parent;
                    }
                    foreach (Vector2i el in pending)
                    {
                        yield return el;
                    }
                }
            }
        }

        public static bool TileInConeRelative(Vector2i relative, Vector2i direction, float sectorDegrees)
        {
            if (relative.x == 0 && relative.y == 0) { return true; }

            // TODO: Experiment with leniency
            // check the middle
            // if (PointInCone((tile.x, tile.y), direction, sectorDegrees)) {return true;}

            // check the four edges.
            // if (PointInCone((tile.x + 0.5f, tile.y), direction, sectorDegrees)) { return true; }
            // if (PointInCone((tile.x - 0.5f, tile.y), direction, sectorDegrees)) { return true; }
            // if (PointInCone((tile.x, tile.y - 0.5f), direction, sectorDegrees)) { return true; }
            // if (PointInCone((tile.x, tile.y - 0.5f), direction, sectorDegrees)) { return true; }

            // check the four corners.
            if (PointInConeRelative((relative.x + 0.5f, relative.y + 0.5f), direction, sectorDegrees)) { return true; }
            if (PointInConeRelative((relative.x - 0.5f, relative.y + 0.5f), direction, sectorDegrees)) { return true; }
            if (PointInConeRelative((relative.x + 0.5f, relative.y - 0.5f), direction, sectorDegrees)) { return true; }
            if (PointInConeRelative((relative.x - 0.5f, relative.y - 0.5f), direction, sectorDegrees)) { return true; }

            return false;
        }

        // TODO: Floating point muckery. Reordering operations increases precision. Or maybe alternate expression?
        private static bool PointInConeRelative((float x, float y) point, Vector2i direction, float sectorDegrees)
        {
            if (point.x == 0 && point.y == 0) { return true; }

            double pointLenSqr = point.x * point.x + point.y * point.y;
            double dirLenSqr = direction.x * direction.x + direction.y * direction.y;
            return Math.Acos((point.x * direction.x + point.y * direction.y) / Math.Sqrt(pointLenSqr * dirLenSqr)) <= sectorDegrees / 2 * Math.PI / 180 + 0.01;
        }

        public static bool AnyLineOfSightRelative(Vector2i relative, Predicate<Vector2i> isBlocked)
        {
            // Any tiles that are duplicated across octants would be covered the same way anyways.
            (int dx, int dy, int octant) = GridHelper.Octantify(relative);

            if (!reverse.ContainsKey((dx, dy))) { return false; }
            foreach (TrieNode start in reverse[(dx, dy)])
            {
                bool success = true;
                TrieNode current = start;
                while (current != origin)
                {
                    if (isBlocked(GridHelper.DeOctantify(current.x, current.y, octant)))
                    {
                        success = false;
                        break;
                    }
                    current = current.parent;
                }
                if (success) { return true; }
            }
            return false;
        }

        // Predicate nonsense.
        private static Predicate<Vector2i> ToRelative(AbsolutePosition from, Predicate<AbsolutePosition> predicate)
        {
            return vec => predicate(from + vec);
        }

        // The user friendly versions.
        public static IEnumerable<AbsolutePosition> FieldOfView(AbsolutePosition center, Predicate<AbsolutePosition> isBlocked, float radius)
        {
            foreach (var tile in FieldOfViewRelative(ToRelative(center, isBlocked), radius))
            {
                yield return center + tile;
            }
        }

        // TODO: Decide if direction makes sense. Actions' targetPos is also absolute.
        public static IEnumerable<AbsolutePosition> ConeOfView(AbsolutePosition center, Predicate<AbsolutePosition> isBlocked, float radius, Vector2i direction, float sectorDegrees)
        {
            foreach (var tile in ConeOfViewRelative(ToRelative(center, isBlocked), radius, direction, sectorDegrees))
            {
                yield return center + tile;
            }
        }

        // TODO: Decide if direction makes sense.
        public static bool TileInCone(AbsolutePosition center, AbsolutePosition target, Vector2i direction, float sectorDegrees)
        {
            return TileInConeRelative(target - center, direction, sectorDegrees);
        }

        public static bool AnyLineOfSight(AbsolutePosition from, AbsolutePosition to, Predicate<AbsolutePosition> isBlocked)
        {
            return AnyLineOfSightRelative(to - from, ToRelative(from, isBlocked));
        }

        // debug. this creates an iterators for every node but idc.
        public static IEnumerable<TrieNode> DumpTree(TrieNode node)
        {
            yield return node;
            if (node.straight is object)
            {
                foreach (TrieNode eee in DumpTree(node.straight))
                {
                    yield return eee;
                }
            }
            if (node.diag is object)
            {
                foreach (TrieNode eee in DumpTree(node.diag))
                {
                    yield return eee;
                }
            }
            if (node.up is object)
            {
                foreach (TrieNode eee in DumpTree(node.diag))
                {
                    yield return eee;
                }
            }
        }
    }
}