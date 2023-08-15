using System;
using System.Collections.Generic;
using System.Linq;

namespace LizardState.Engine
{
    // Algorithms and Math.
    static class GridHelper
    {
        public static IEnumerable<Vector2i> GetDirections()
        {
            yield return Vector2i.LEFT;
            yield return Vector2i.RIGHT;
            yield return Vector2i.UP;
            yield return Vector2i.DOWN;
            yield return Vector2i.LEFT + Vector2i.UP;
            yield return Vector2i.RIGHT + Vector2i.UP;
            yield return Vector2i.LEFT + Vector2i.DOWN;
            yield return Vector2i.RIGHT + Vector2i.DOWN;
        }

        public static IEnumerable<AbsolutePosition> GetNeighbors(AbsolutePosition a)
        {
            return GetDirections().Select(dir => a + dir);
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

        const int RAY_LENGTH = 500;

        // Not necessarily symmetric!
        public static IEnumerable<AbsolutePosition> RayThrough(AbsolutePosition from, AbsolutePosition through)
        {
            yield return from;
            if (from == through) { yield break; }

            (int octantX, int octantY, int octant) = Octantify(through - from);
            int localDy = 0;
            for (int localDx = 1; localDx <= RAY_LENGTH; localDx++)
            {
                if (octantX * (2 * localDy + 1) < 2 * octantY * localDx)
                {
                    localDy++;
                }
                yield return from + DeOctantify(localDx, localDy, octant);
            }
        }

        // I've understood the algorithm and modified to force symmetry, but its good to cite still I suppose.
        // Original algorithm found in https://github.com/denismr/SymmetricPCVT
        // Tran Thong "A symmetric linear algorithm for line segment generation."
        public static IEnumerable<AbsolutePosition> LineBetween(AbsolutePosition from, AbsolutePosition to)
        {
            yield return from;
            if (from == to) { yield break; }

            (int octantX, int octantY, int octant) = Octantify(to - from);
            int localDy = 0;
            for (int localDx = 1; localDx <= octantX; localDx++)
            {
                // force symmetry in the exact center.
                if (octantY % 2 != 0 && localDx * 2 == octantX)
                {
                    yield return from + DeOctantify(localDx, localDy, octant);
                }

                // rotate slightly around midpoint.
                // if ((2 * localDy - octantY + 1) <= ((float)octantY / octantX - 0.001) * (2 * localDx - octantX))
                // we can simplify and make int only. scale, foil the right, avoid division.
                if (octantX * (1024 * (2 * localDy - octantY + 1) + (2 * localDx - octantX)) <= 1024 * octantY * (2 * localDx - octantX))
                {
                    localDy++;
                }
                yield return from + DeOctantify(localDx, localDy, octant);
            }
        }

        public static AbsolutePosition StepThrough(AbsolutePosition from, AbsolutePosition through, int distance)
        {
            IEnumerable<AbsolutePosition> enumerable = RayThrough(from, through);
            AbsolutePosition previous = from;
            foreach (AbsolutePosition p in enumerable)
            {
                if (Distance(p - from) > distance) { return previous; }
                previous = p;
            }
            // this will never happen.
            return through;
        }

        public static AbsolutePosition StepTowards(AbsolutePosition from, AbsolutePosition to, int distance)
        {
            IEnumerable<AbsolutePosition> enumerable = LineBetween(from, to);
            AbsolutePosition previous = from;
            foreach (AbsolutePosition p in enumerable)
            {
                if (Distance(p - from) > distance) { return previous; }
                previous = p;
            }
            return to;
        }

        [Obsolete]
        public static (int dx, int dy, int octant) Octantify(int dx, int dy)
        {
            return Octantify((dx, dy));
        }

        public static (int dx, int dy, int octant) Octantify(Vector2i delta)
        {
            (int dx, int dy) = (delta.x, delta.y);
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

        public static Vector2i DeOctantify(int dx, int dy, int octant)
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
            return new Vector2i(dx, dy);
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
            if (a % b == 0) { return b; }
            return GCD(b, a % b);
        }

        public static int Distance(int dx, int dy)
        {
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);

            // Chebyshev
            return Math.Max(dx, dy);

            // Approximate Approximate Euclidean
            // return Math.Max(dx, dy) + 0.5f * Math.Min(dx, dy);
            // return (int)(Math.Max(dx, dy) + 0.5f * Math.Min(dx, dy));
        }

        public static int Distance(Vector2i delta)
        {
            return Distance(delta.x, delta.y);
        }

        public static int Distance(AbsolutePosition pos, AbsolutePosition pos2)
        {
            return Distance(pos - pos2);
        }
    }
}