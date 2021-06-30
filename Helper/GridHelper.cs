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
        yield return (a.x - 1, a.y - 1);
        yield return (a.x + 1, a.y - 1);
        yield return (a.x - 1, a.y + 1);
        yield return (a.x + 1, a.y + 1);
    }

    public static IEnumerable<(int numerator, int denominator)> ListRationals(int radius)
    {
        for (int denom = 1; denom <= radius; denom++)
        {
            for (int numer = 0; numer <= denom; numer++)
            {
                if (GCD(denom, numer) == 1)
                {
                    int scale = (int)((float)radius/denom);
                    yield return (scale * numer, scale * denom);
                }
            }
        }
    }

    public static IEnumerable<(int x, int y)> LineBetween((int x, int y) from, (int x, int y) to)
    {
        if (to.x == from.x && to.y == from.y)
        {
            yield return from;
            yield break;
        }

        (int octantX, int octantY, int octant) = Octantify(to.x - from.x, to.y - from.y);

        float accumulator = 0.5f;
        for (int i = 0; i <= octantX; i++)
        {
            (int dx, int dy) = DeOctantify(i, (int)accumulator, octant);
            yield return (dx + from.x, dy + from.y);
            accumulator += (float)octantY / octantX;
        }        
    }

    public static (int dx, int dy, int octant) Octantify(int dx, int dy)
    {
        int octant = 0;
        if (dy < 0)
        {
            octant += 4;
            (dx, dy) = (-dx, -dy); // rotate 180
        }
        // dx and dy are in quadrants I and II.
        if (dx < 0)
        {
            octant += 2;
            (dx, dy) = (dy, -dx); // rotate 90 clockwise
        }
        // dx and dy are in quadrant I.
        if (dy > dx)
        {
            octant += 1;
            (dx, dy) = (dy, dx); // flip along diagonal y = x.
        }
        return (dx, dy, octant);
    }

    public static (int dx, int dy) DeOctantify(int dx, int dy, int octant)
    {
        if ((octant & 0b001) > 0)
        {
            (dx, dy) = (dy, dx); // flip along diagonal y = x.
        }
        if ((octant & 0b010) > 0)
        {
            (dx, dy) = (-dy, dx); // rotate 90 counterclockwise
        }
        if ((octant & 0b100) > 0)
        {
            (dx, dy) = (-dx, -dy); // rotate 180
        }
        return (dx, dy);
    }

    public static int GCD(int a, int b)
    {
        if (a < b)
        {
            return GCD(b, a);
        }
        if (b == 0) { return a; }

        bool evenA = (a % 2) == 0;
        bool evenB = (b % 2) == 0;
        if (evenA)
        {
            if (evenB)
            {
                return 2 * GCD(a/2, b/2);
            }
            else
            {
                return GCD(a/2, b);
            }
        }
        else
        {
            if (evenB)
            {
                return GCD(a, b/2);
            }
            else
            {
                return GCD(a-b, b);
            }
        }
    }

    public static int Distance((int x, int y) pos, (int x, int y) pos2)
    {
        // return Math.Abs(pos.x - pos2.x) + Math.Abs(pos.y - pos2.y); // Taxicab
        return Math.Max(Math.Abs(pos.x - pos2.x), Math.Abs(pos.y - pos2.y)); // Not Taxicab
        
        // Approximate Euclidean
        // return Math.Max(Math.Abs(pos.x - pos2.x), Math.Abs(pos.y - pos2.y)) + 0.5 * Math.Max(Math.Abs(pos.x - pos2.x), Math.Abs(pos.y - pos2.y));
    }
}