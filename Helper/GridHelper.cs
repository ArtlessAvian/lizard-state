using System;
using System.Collections.Generic;

// Algorithms and Math.
static class GridHelper
{
    public static IEnumerable<(int, int)> GetNeighbors((int x, int y) a)
    {
        // lol lazy
        yield return (a.x - 1, a.y);
        yield return (a.x + 1, a.y);
        yield return (a.x, a.y - 1);
        yield return (a.x, a.y + 1);
        // yield return (a.x - 1, a.y - 1);
        yield return (a.x + 1, a.y - 1);
        yield return (a.x - 1, a.y + 1);
        // yield return (a.x + 1, a.y + 1);
    }

    public static IEnumerable<(int numerator, int denominator)> ListRationals(int maxDenominator)
    {
        // it doesn't make sense to say 0 is coprime with anything.
        yield return (0, 1);
        for (int denom = 1; denom <= maxDenominator; denom++)
        {
            for (int numer = 1; numer <= denom; numer++)
            {
                if (GCD(denom, numer) == 1) // if coprime
                {
                    yield return (numer, denom);
                }
            }
        }
    }

    // Found a better algorithm from https://github.com/denismr/SymmetricPCVT
    // Tran Thong "A symmetric linear algorithm for line segment generation."
    public static IEnumerable<(int x, int y)> RayThrough((int x, int y) from, (int x, int y) through)
    {
        (int octantX, int octantY, int octant) = Octantify(through.x - from.x, through.y - from.y);
        int localDy = 0;
        for (int localDx = 0; localDx <= 100; localDx++)
        {
            (int dx, int dy) = DeOctantify(localDx, localDy, octant);
            yield return (dx + from.x, dy + from.y);
            if (octantX * (localDy + 0.5f) - octantY * (localDx + 1) < 0)
            {
                localDy++;
            }
        }
    }
    
    public static IEnumerable<(int x, int y)> LineBetween((int x, int y) from, (int x, int y) to)
    {
        foreach ((int x, int y) p in RayThrough(from, to))
        {
            yield return p;
            if (p.x == to.x && p.y == to.y)
            {
                break;
            }
        }
    }

    public static (int dx, int dy, int octant) Octantify(int dx, int dy)
    {
        // convert to cube coordinates, and permute. q + r + s = 0 always
        int dodecant = 0; // theres only 6 permutations, but im going to use 8 values.
        int q = dx;
        int r = dy;
        int s = -dx - dy;

        if (q < r)
        {
            (q, r) = (r, q);
            dodecant += 1;
        }
        if (r < s)
        {
            (r, s) = (s, r); // s is now least
            dodecant += 2;
        }
        if (q < r)
        {
            (q, r) = (r, q); // qrs is sorted.
            dodecant += 4;
        }
        if (r < 0)
        {
            r = -r;
            dodecant += 8;
        }

        return (q, r, dodecant);
    }

    public static (int dx, int dy) DeOctantify(int dx, int dy, int octant)
    {
        int q = dx;
        int r = dy;
        int s = -dx - dy;

        if ((octant & 0b1000) > 0)
        {
            r = -r;
        }
        if ((octant & 0b100) > 0)
        {
            (q, r) = (r, q);
        }
        if ((octant & 0b010) > 0)
        {
            (r, s) = (s, r);
        }
        if ((octant & 0b001) > 0)
        {
            (q, r) = (r, q);
        }
        return (q, r);
    }

    // Euclidean algorithm :P
    // Literally /who/ told me the previous algorithm?
    // im actually mad that i overcomplicated it. don't search it up.
    public static int GCD(int a, int b)
    {
        if (a < b)
        {
            return GCD(b, a);
        }
        if (b == 0) { return 0; } // this should never happen!!
        if (a % b == 0) {return b;}
        return GCD(b, a % b);
    }

    public static int Distance(int dx, int dy)
    {
        // hexagon!!
        return (Math.Abs(dx) + Math.Abs(dx + dy) + Math.Abs(dy))/2;

        // dx = Math.Abs(dx);
        // dy = Math.Abs(dy);

        // Chebyshev
        // return Math.Max(dx, dy);
        
        // Approximate Approximate Euclidean
        // return Math.Max(dx, dy) + 0.5f * Math.Min(dx, dy);
        // return (int)(Math.Max(dx, dy) + 0.5f * Math.Min(dx, dy));
    }

    public static int Distance((int x, int y) pos, (int x, int y) pos2)
    {
        return Distance(pos.x - pos2.x, pos.y - pos2.y);
    }
}