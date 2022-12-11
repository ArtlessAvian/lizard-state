
using System;
using System.Collections.Generic;
using Godot;

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
            foreach ((int x, int y) in GridHelper.RayThrough((0, 0), (run, rise)))
            {
                if (x == 0) { continue; } // skip the first one.
                if (x > radius) { break; } // finish up
                if (y == current.y)
                {
                    if (current.straight is null)
                    {
                        current.straight = new TrieNode(x, y, current);
                        AddToReverse(current.straight);
                    }
                    current = current.straight;
                }
                else
                {
                    if (current.diag is null)
                    {
                        current.diag = new TrieNode(x, y, current);
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
    public static IEnumerable<(int relX, int relY)> FieldOfViewRelative(Predicate<(int relX, int relY)> isBlocked, float radius)
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

                (int, int) relativePos = GridHelper.DeOctantify(current.x, current.y, octant);
                yield return relativePos;
                if (!isBlocked(relativePos))
                {
                    stack.Add(current.straight);
                    stack.Add(current.diag);
                }
            }
        }
    }

    public static IEnumerable<(int relX, int relY)> ConeOfViewRelative(Predicate<(int relX, int relY)> isBlocked, float radius, (int x, int y) direction, float sectorDegrees)
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

                (int, int) relativePos = GridHelper.DeOctantify(current.x, current.y, octant);
                if (TileInConeRelative(relativePos, direction, sectorDegrees))
                {
                    yield return relativePos;
                }

                if (!isBlocked(relativePos))
                {
                    stack.Add(current.straight);
                    stack.Add(current.diag);
                }
            }
        }
    }

    public static bool TileInConeRelative((int x, int y) relative, (int x, int y) direction, float sectorDegrees)
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
    private static bool PointInConeRelative((float x, float y) point, (int x, int y) direction, float sectorDegrees)
    {
        if (point.x == 0 && point.y == 0) { return true; }

        double pointLenSqr = point.x * point.x + point.y * point.y;
        double dirLenSqr = direction.x * direction.x + direction.y * direction.y;
        return Math.Acos((point.x * direction.x + point.y * direction.y) / Math.Sqrt(pointLenSqr * dirLenSqr)) <= sectorDegrees / 2 * Math.PI / 180 + 0.01;
    }

    public static bool AnyLineOfSightRelative((int x, int y) relative, Predicate<(int relX, int relY)> isBlocked)
    {
        // Any tiles that are duplicated across octants would be covered the same way anyways.
        (int dx, int dy, int octant) = GridHelper.Octantify(relative.x, relative.y);

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
    // Malding. Be careful to pick the right one.
    private static (int relX, int relY) GetOffset((int x, int y) from, (int x, int y) to)
    {
        return (to.x - from.x, to.y - from.y);
    }

    private static (int x, int y) AddOffset((int x, int y) from, (int relX, int relY) offset)
    {
        return (from.x + offset.relX, from.y + offset.relY);
    }

    private static Predicate<(int relX, int relY)> ToRelative((int x, int y) from, Predicate<(int x, int y)> predicate)
    {
        return ((int relX, int relY) offset) => predicate(AddOffset(from, offset));
    }

    // The user friendly versions.
    public static IEnumerable<(int x, int y)> FieldOfView((int x, int y) center, Predicate<(int absX, int absY)> isBlocked, float radius)
    {
        foreach (var a in FieldOfViewRelative(ToRelative(center, isBlocked), radius))
        {
            yield return AddOffset(center, a);
        }
    }

    // TODO: Decide if direction makes sense. Actions' targetPos is also absolute.
    public static IEnumerable<(int x, int y)> ConeOfView((int x, int y) center, Predicate<(int x, int y)> isBlocked, float radius, (int x, int y) direction, float sectorDegrees)
    {
        foreach (var a in ConeOfViewRelative(ToRelative(center, isBlocked), radius, direction, sectorDegrees))
        {
            yield return AddOffset(center, a);
        }
    }

    // TODO: Decide if direction makes sense.
    public static bool TileInCone((int x, int y) center, (int x, int y) target, (int, int) direction, float sectorDegrees)
    {
        return TileInConeRelative(GetOffset(center, target), direction, sectorDegrees);
    }

    public static bool AnyLineOfSight((int x, int y) from, (int x, int y) to, Predicate<(int x, int y)> isBlocked)
    {
        return AnyLineOfSightRelative(GetOffset(from, to), ToRelative(from, isBlocked));
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
    }
}