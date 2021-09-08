
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

        public TrieNode(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // public IEnumerable<TrieNode> DumpTree()
        // {
        //     yield return this;
        //     if (straight is object)
        //     {
        //         foreach (TrieNode eee in straight.DumpTree())
        //         {
        //             yield return eee;
        //         }
        //     }
        //     if (diag is object)
        //     {
        //         foreach (TrieNode eee in diag.DumpTree())
        //         {
        //             yield return eee;
        //         }
        //     }
        // }
    }

    public TrieNode origin = new TrieNode(0, 0);

    private int currentRadius = 0;

    public void ExtendRadius(int radius)
    {
        if (this.currentRadius >= radius) { return; }
        this.currentRadius = radius;
        
        // For each unique slope passing through a cell,
        foreach ((int rise, int run) in GridHelper.ListRationals(radius))
        {
            // extend the ray to the end of the square
            int thing = radius * rise / run;

            TrieNode current = origin;
            foreach ((int x, int y) in GridHelper.LineBetween((0, 0), (radius, thing)))
            {
                if (x == 0) {continue;} // skip the first one.
                if (y == current.y)
                {
                    if (current.straight is null)
                    {
                        current.straight = new TrieNode(x, y);
                    }
                    current = current.straight;
                }
                else
                {
                    if (current.diag is null)
                    {
                        current.diag = new TrieNode(x, y);
                    }
                    current = current.diag;
                }
            }
        }
    }
}