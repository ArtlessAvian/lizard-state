using Godot;
using System;
using System.Collections.Generic;

// A random planar graph.
// Distribution is, something. Not sure if it creates every graph but whatever.
class PlanarGraph
{
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    [Export] public int nodes;
    [Export] public int maxDegree; // geq to 2.
    [Export] public int diameter; // loose constraint, will break to keep degree.
    [Export] public bool isTree;
    [Export] public int seed;

    public List<int>[] edges;
    public List<int>[] subtreeChildren;
    public int[] subtreeDepth;

    public PlanarGraph(int nodes, int maxDegree, int diameter, bool isTree = false, ulong? seed = null)
    {
        this.nodes = nodes;
        this.maxDegree = maxDegree;
        this.diameter = diameter;
        this.isTree = isTree;
        rng.Seed = seed ?? 413;

        edges = new List<int>[nodes];
        subtreeChildren = new List<int>[nodes];
        for (int i = 0; i < nodes; i++)
        {
            edges[i] = new List<int>();
            subtreeChildren[i] = new List<int>();
        }
        subtreeDepth = new int[nodes];

        // MessWithDiameter();
        CreateTree();
        if (!isTree) { AddCrossEdges(); }
    }

    private void CreateTree()
    {
        // "BFS" a tree.
        int[] numSubnodes = new int[nodes];
        bool[] extraDepth = new bool[nodes]; // this subtree can be one tall extra.

        subtreeDepth[0] = 0;
        numSubnodes[0] = nodes - 1;
        int maxDiscovered = 0;

        for (int node = 0; node < nodes; node++)
        {
            // if no subnodes, leaf.
            if (numSubnodes[node] <= 0) { continue; }

            // decide how many branches there are.
            int minBranches, maxBranches;
            if (node == 0)
            {
                minBranches = MinBranchesRoot(numSubnodes[node], maxDegree, diameter);
                maxBranches = maxDegree;
            }
            else
            {
                int subTreeHeight = (diameter - 1) / 2 + (extraDepth[node] ? 1 : 0) - subtreeDepth[node];
                minBranches = MinBranches(numSubnodes[node], maxDegree, subTreeHeight);
                maxBranches = maxDegree - 1;
            }
            if (minBranches < 1)
            {
                GD.PrintErr("minBranches < 1. Cannot fit tree inside diameter.");
                minBranches = 1;
            }
            maxBranches = Math.Min(maxBranches, numSubnodes[node]);
            if (minBranches > maxBranches)
            {
                GD.PrintErr("minBranches > maxBranches. breaking diameter constraint (keeping degree)");
                minBranches = maxBranches;
            }

            // int branches = minBranches;
            int branches = rng.RandiRange(minBranches, maxBranches);
            int firstChild = maxDiscovered + 1;
            maxDiscovered += branches;

            // add edges
            for (int i = firstChild; i <= maxDiscovered; i++)
            {
                edges[node].Add(i);
                edges[i].Add(node);
                subtreeChildren[node].Add(i);
                subtreeDepth[i] = subtreeDepth[node] + 1;
            }

            // decide how to divide the subnodes.
            // TODO: divide the trees /validly/.
            int minInSubdivision = 0;
            int slack = numSubnodes[node] - branches;
            List<int> divisions = new List<int>();
            divisions.Add(0);
            for (int _ = 0; _ < branches - 1; _++)
            {
                divisions.Add(rng.RandiRange(0, slack));
            }
            divisions.Add(slack);
            divisions.Sort();

            List<int> sizes = new List<int>();
            for (int i = 0; i < branches; i++)
            {
                sizes.Add(divisions[i + 1] - divisions[i] + minInSubdivision);
            }

            for (int i = 0; i < branches; i++)
            {
                numSubnodes[firstChild + i] = divisions[i + 1] - divisions[i];
            }
            if (extraDepth[node] || (node == 0 && diameter % 2 == 1))
            {
                extraDepth[firstChild] = true;
            }
        }
    }

    // minimum branches a subtree of height height must have to fit subnodes subnodes.
    // one branch is allowed to be extra long with extraDepth.
    private static int MinBranches(int subnodes, int maxDegree, int height)
    {
        // get the max size of a tree of height (height - 1) and branching factor (maxDegree - 1).
        int subTreeSize = 1;
        for (int h = 1; h <= height - 1; h++)
        {
            subTreeSize = 1 + (maxDegree - 1) * subTreeSize;
        }
        return (int)Math.Ceiling((double)subnodes / subTreeSize);
    }

    private static int MinBranchesRoot(int subnodes, int maxDegree, int diameter)
    {
        int smallTreeSize = 1;
        {
            int smallTreeHeight = diameter / 2 - 1;
            for (int height = 1; height <= smallTreeHeight; height++)
            {
                smallTreeSize = 1 + (maxDegree - 1) * smallTreeSize;
            }
        }
        if (diameter % 2 == 0)
        {
            // How many small trees?
            return (int)Math.Ceiling((double)subnodes / smallTreeSize);
        }
        else
        {
            int bigTreeSize = 1 + (maxDegree - 1) * smallTreeSize;
            if (subnodes <= bigTreeSize) { return 1; }
            // How many branches with one big tree and n small trees?
            return (int)Math.Ceiling((double)(subnodes - bigTreeSize) / smallTreeSize) + 1;
        }
    }

    private void AddCrossEdges()
    {
        AddCrossEdgesDFS(0, new List<int>());
    }

    private void AddCrossEdgesDFS(int node, List<int> targets)
    {
        // Decide to add edges from node to some targets.
        int? maxTarget = null;
        while ((targets.Count > 0) && (edges[node].Count < maxDegree) && (rng.Randf() < 0.5))
        {
            int target = targets[targets.Count - 1];
            // int target = targets[rng.RandiRange(0, targets.Count - 1)];
            edges[node].Add(target);
            edges[target].Add(node);

            targets.RemoveAll(val => val <= target);

            if (maxTarget == null || target > maxTarget)
            {
                maxTarget = target;
            }
        }
        if (maxTarget is int readd)
        {
            targets.Add(readd);
        }

        if (subtreeChildren[node].Count == 0)
        {
            targets.Clear();
        }
        else
        {
            foreach (int child in subtreeChildren[node])
            {
                AddCrossEdgesDFS(child, targets);
            }
        }
        // When rewinding, add to targets.
        if (edges[node].Count < maxDegree)
        {
            targets.Add(node);
        }
    }

    public void DumpGraph()
    {
        GD.PrintS("this is a", nodes, "graph with maximum degree", maxDegree, "and (ideally) diameter", diameter);

        // dfs through subtree
        string dashes = "-------------------------";

        List<int> frontier = new List<int>();
        frontier.Add(0);
        while (frontier.Count > 0)
        {
            int current = frontier[frontier.Count - 1];
            frontier.RemoveAt(frontier.Count - 1);
            GD.PrintS(dashes.Substr(0, subtreeDepth[current]), current, "(" + string.Join<int>(",", edges[current]) + ")");
            for (int i = subtreeChildren[current].Count - 1; i >= 0; i--)
            {
                frontier.Add(subtreeChildren[current][i]);
            }
        }
    }

    // god i wish i had a proper testing suite
    public static void TestMe()
    {
        GD.Print("Perfect Binary Tree");
        GD.Print("Should be 2: ", MinBranches(2, 3, 1));
        GD.Print("Should be 3: ", MinBranches(3, 3, 1));
        GD.Print("Should be 2: ", MinBranchesRoot(2, 3, 2));
        GD.Print("Should be 3: ", MinBranchesRoot(3, 3, 2));
        GD.Print("Should be 4: ", MinBranchesRoot(4, 3, 2));
        GD.Print("Should be 2: ", MinBranches(6, 3, 2));
        GD.Print("Should be 3: ", MinBranches(7, 3, 2));
        GD.Print("Should be 2: ", MinBranchesRoot(6, 3, 4));
        GD.Print("Should be 3: ", MinBranchesRoot(7, 3, 4));
        GD.Print("Should be 2: ", MinBranches(126, 3, 6));
        GD.Print("Should be 3: ", MinBranches(127, 3, 6));
        GD.Print("Should be 2: ", MinBranchesRoot(126, 3, 12));
        GD.Print("Should be 3: ", MinBranchesRoot(127, 3, 12));

        GD.Print("Complete Binary Tree");
        GD.Print("Should be 1: ", MinBranchesRoot(1, 3, 1));
        GD.Print("Should be 1 (invalid): ", MinBranchesRoot(2, 3, 1));
        GD.Print("Should be 2: ", MinBranchesRoot(4, 3, 3));
        GD.Print("Should be 3: ", MinBranchesRoot(5, 3, 3));
        GD.Print("Should be 2: ", MinBranchesRoot(10, 3, 6));
        GD.Print("Should be 2: ", MinBranchesRoot(190, 3, 14));

        // totally not mh generations jurassic frontier
        PlanarGraph graph = new PlanarGraph(12, 4, 5, false);
        graph.DumpGraph();
    }
}