
using System;
using System.Collections.Generic;

// See http://www.roguebasin.com/index.php?title=Pre-Computed_Visibility_Tries
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

    public TrieNode origin;

    private float currentRadius = 0;
    public Dictionary<(int x, int y), HashSet<TrieNode>> reverse;

    public VisibilityTrie()
    {
        origin = new TrieNode(0, 0);
        reverse = new Dictionary<(int x, int y), HashSet<TrieNode>>();
        AddToReverse(origin);
    }

    public void ExtendRadius(float radius)
    {
        if (this.currentRadius >= radius) { return; }
        this.currentRadius = radius;
        
        // For each unique ray in the octant passing through a cell,
        foreach ((int rise, int run) in GridHelper.ListRationals((int)radius + 1))
        {
            // Add the tiles the ray hits to the trie.
            TrieNode current = origin;
            foreach ((int x, int y) in GridHelper.RayThrough((0, 0), (run, rise)))
            {
                if (x == 0) {continue;} // skip the first one.
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

    public void AddToReverse(TrieNode node)
    {
        (int x, int y) pos = (node.x, node.y);
        if (!reverse.ContainsKey(pos))
        {
            reverse.Add(pos, new HashSet<TrieNode>());
        }
        reverse[pos].Add(node);
    }

    /// May yield duplicates! If critical, use a set.
    public IEnumerable<(int relX, int relY)> FieldOfView(Predicate<(int relX, int relY)> isBlocked, int radius)
    {
        // Note that recurring has awful performance, but its easier. See the dumptree function.
        for (int octant = 0; octant < 8; octant++)
        {
            List<TrieNode> stack = new List<TrieNode>{origin};
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

    public bool AnyLineOfSight((int x, int y) relative, Predicate<(int relX, int relY)> isBlocked)
    {
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

    // debug
    public IEnumerable<TrieNode> DumpTree(TrieNode node)
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