using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

// A random planar graph.
// This cannot generate every graph. Example: verticies of a cube.
// I have decided to intentionally not do that, because that would be unfun.
class PlanarGraph
{
    const int NODE_LIMIT = 200; // no way there'll be a 200 node graph.

    private RandomNumberGenerator rng = new RandomNumberGenerator();
    public readonly int nodes;
    public readonly int maxDegree; // geq to 2.
    public readonly int diameter; // loose constraint, will break to keep degree.
    public readonly bool isTree;
    public readonly int seed;

    public List<int>[] edges;

    // implementation details.
    public List<int>[] subtreeChildren;
    public int[] subtreeDepth;

    public PlanarGraph(int nodes, int maxDegree, int diameter, bool isTree = false, ulong? seed = null)
    {
        // mess with diameter
        int original = diameter;
        while (GetMinBranches(nodes - 1, maxDegree - 1, diameter) > maxDegree - 1)
        {
            diameter++;
        }
        if (original != diameter) { GD.PrintErr($"Diameter increased from {original} to {diameter}!"); }

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

        CreateTree();
        if (!isTree) { AddCrossEdges(); }
    }

    private void CreateTree()
    {
        // "BFS" a tree.
        int[] numSubnodes = new int[nodes];
        int[] subtreeDiameter = new int[nodes]; // this subtree can be one tall extra.

        subtreeDepth[0] = 0;
        numSubnodes[0] = nodes - 1;
        subtreeDiameter[0] = diameter;
        int maxDiscovered = 0;

        for (int node = 0; node < nodes; node++)
        {
            // if no subnodes, leaf.
            if (numSubnodes[node] <= 0) { continue; }

            // decide how many branches there are.
            int minBranches = GetMinBranches(numSubnodes[node], maxDegree - 1, subtreeDiameter[node]);
            int maxBranches = maxDegree - 1;
            maxBranches = Math.Min(maxBranches, numSubnodes[node]);

            // int branches = minBranches;
            int branches = rng.RandiRange(minBranches, maxBranches);
            int firstChild = maxDiscovered + 1;
            maxDiscovered += branches;

            GD.PrintS(minBranches, maxBranches, branches);

            // add edges
            for (int i = firstChild; i <= maxDiscovered; i++)
            {
                edges[node].Add(i);
                edges[i].Add(node);
                subtreeChildren[node].Add(i);
                subtreeDepth[i] = subtreeDepth[node] + 1;

                subtreeDiameter[i] = ((subtreeDiameter[node] - 2) / 2) * 2;
            }
            GD.Print(subtreeDiameter[node]);

            // decide how to divide the subnodes.
            int remaining = numSubnodes[node] - branches;
            List<int> sizes = new List<int>();
            for (int i = 0; i < branches - 1; i++)
            {
                int minSize = GetMinInSubdivision(remaining, maxDegree - 1, subtreeDiameter[node], branches - i);
                int maxSize = Math.Min(remaining, GetTreeSize(maxDegree - 1, subtreeDiameter[node] / 2 - 1));
                int size = rng.RandiRange(minSize, maxSize);
                remaining -= size;
                sizes.Add(size);
            }
            sizes.Add(remaining);

            for (int i = 0; i < branches; i++)
            {
                numSubnodes[firstChild + i] = sizes[i];
            }
            if (node == 0 && diameter % 2 == 1)
            {
                int maxIndex = sizes.IndexOf(sizes.Max());
                subtreeDiameter[firstChild + maxIndex] += 2;
            }
        }
    }

    // returns the number of branches needed to meet the two requirements.
    private static int GetMinBranches(int subnodes, int branchingFactor, int subtreeDiameter)
    {
        int childHeight = subtreeDiameter / 2 - 1;
        int childSize = GetTreeSize(branchingFactor, childHeight);
        if (subtreeDiameter % 2 == 0)
        {
            // easy case. all subtrees are same size.
            // how many small trees?
            return (int)Math.Ceiling((double)subnodes / childSize);
        }
        int bigChildSize = 1 + branchingFactor * childSize;
        if (subnodes <= bigChildSize) { return 1; } // everything fits in the big tree.
        // How many branches with one big tree and n small trees?
        return (int)Math.Ceiling((double)(subnodes - bigChildSize) / childSize) + 1;
    }

    private int GetMinInSubdivision(int subnodes, int branchingFactor, int subtreeDiameter, int branches)
    {
        int childHeight = subtreeDiameter / 2 - 1;
        int childSize = GetTreeSize(branchingFactor, childHeight);
        if (subtreeDiameter % 2 == 0)
        {
            return Math.Max(0, subnodes - (branches - 1) * childSize);
        }
        int bigChildSize = 1 + branchingFactor * childSize;
        // shove everything into the big child and the (branch-2) small children.
        return (int)Math.Max(0, subnodes - bigChildSize - childSize * (branches - 2));
    }

    private static int GetTreeSize(int branchingFactor, int height)
    {
        int size = 1;
        for (int h = 1; h <= height; h++)
        {
            size = 1 + branchingFactor * size;
            if (size >= NODE_LIMIT)
            {
                return size;
            }
        }
        return size;
    }

    private void AddCrossEdges()
    {
        AddCrossEdgesDFS(0, new List<int>());
    }

    private void AddCrossEdgesDFS(int node, List<int> targets)
    {
        // Decide to add edges from node to some targets.
        // TODONT: To make every planar graph possible, keep track of all nodes on the outer face.
        int? maxTarget = null;
        while ((targets.Count > 0) && (edges[node].Count < maxDegree) && (rng.Randf() < 0.5))
        {
            // int target = targets[targets.Count - 1];
            // int target = targets[rng.RandiRange(0, targets.Count - 1)];
            // Select a node of equal depth, or one greater.
            int equalDepth = targets.Find(index => subtreeDepth[index] == subtreeDepth[node]);
            int inequalDepth = targets.Find(index => subtreeDepth[index] == subtreeDepth[node] + 1);
            int target = rng.Randf() < 0.5 ? equalDepth : inequalDepth;
            // HACK: targets.Find returns 0 if nothing matches. Nothing can link to the root anyways. I'm tired.
            if (target != 0)
            {
                edges[node].Add(target);
                edges[target].Add(node);

                targets.RemoveAll(val => val <= target);

                if (maxTarget == null || target > maxTarget)
                {
                    maxTarget = target;
                }
            }
        }
        if (maxTarget is int maxTargett)
        {
            targets.Add(maxTargett); 
            targets.RemoveAll(val => edges[val].Count >= maxDegree);
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
        GD.Print("Should be 2: ", GetMinBranches(4, 2, 3));
        GD.Print("Should be 3: ", GetMinBranches(5, 2, 3));
        GD.Print("Should be 2: ", GetMinBranches(5, 3, 3));
        GD.Print("Should be 3: ", GetMinBranches(6, 3, 3));
        GD.Print("Should be 2: ", GetMinBranches(6, 4, 3));
        GD.Print("Should be 3: ", GetMinBranches(7, 4, 3));

        GD.Print("Should be 2: ", GetMinBranches(6, 2, 4));
        GD.Print("Should be 3: ", GetMinBranches(7, 2, 4));
        GD.Print("Should be 2: ", GetMinBranches(8, 3, 4));
        GD.Print("Should be 3: ", GetMinBranches(9, 3, 4));
        GD.Print("Should be 2: ", GetMinBranches(10, 4, 4));
        GD.Print("Should be 3: ", GetMinBranches(11, 4, 4));

        // GD.Print("Perfect Binary Tree");
        // GD.Print("Should be 2: ", MinBranches(2, 3, 1));
        // GD.Print("Should be 3: ", MinBranches(3, 3, 1));
        // GD.Print("Should be 2: ", MinBranchesRoot(2, 3, 2));
        // GD.Print("Should be 3: ", MinBranchesRoot(3, 3, 2));
        // GD.Print("Should be 4: ", MinBranchesRoot(4, 3, 2));
        // GD.Print("Should be 2: ", MinBranches(6, 3, 2));
        // GD.Print("Should be 3: ", MinBranches(7, 3, 2));
        // GD.Print("Should be 2: ", MinBranchesRoot(6, 3, 4));
        // GD.Print("Should be 3: ", MinBranchesRoot(7, 3, 4));
        // GD.Print("Should be 2: ", MinBranches(126, 3, 6));
        // GD.Print("Should be 3: ", MinBranches(127, 3, 6));
        // GD.Print("Should be 2: ", MinBranchesRoot(126, 3, 12));
        // GD.Print("Should be 3: ", MinBranchesRoot(127, 3, 12));

        // GD.Print("Complete Binary Tree");
        // GD.Print("Should be 1: ", MinBranchesRoot(1, 3, 1));
        // GD.Print("Should be 1 (invalid): ", MinBranchesRoot(2, 3, 1));
        // GD.Print("Should be 2: ", MinBranchesRoot(4, 3, 3));
        // GD.Print("Should be 3: ", MinBranchesRoot(5, 3, 3));
        // GD.Print("Should be 2: ", MinBranchesRoot(10, 3, 6));
        // GD.Print("Should be 2: ", MinBranchesRoot(190, 3, 14));

        // // totally not mh generations jurassic frontier
        // PlanarGraph graph = new PlanarGraph(12, 4, 5, false);
        // graph.DumpGraph();
    }
}