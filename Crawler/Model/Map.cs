using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class Map
{
    // Generated
    Dictionary generatorData;
    public TileMap map; // hehe parasitic inheritance.

    public Map()
    {
        map = new TileMap();
    }

    // Copy Pasted from Previous Iteration of Project
    // public void UpdateVisibility()
    // {
    //     // Every visible tile is now at least revealed.
    //     foreach (Vector2 vec in GetUsedCellsById(VISIBLE))
    //     {
    //         SetCellv(vec, REVEALED);
    //     }
    //     Crawler crawler = GetParent().GetParent<Crawler>();
    //     Entity entity = crawler.GetPlayer();
    //     // For every unique slope passing through a cell,
    //     foreach ((int x, int y) in ListRationals(6))
    //     {
    //         // Mark every cell on that slope, for each of the 8 octants.
    //         MarkLineOfSight((entity.x, entity.y), (entity.x + x, entity.y + y));
    //         MarkLineOfSight((entity.x, entity.y), (entity.x - x, entity.y + y));
    //         MarkLineOfSight((entity.x, entity.y), (entity.x + x, entity.y - y));
    //         MarkLineOfSight((entity.x, entity.y), (entity.x - x, entity.y - y));
    //         MarkLineOfSight((entity.x, entity.y), (entity.x + y, entity.y + x));
    //         MarkLineOfSight((entity.x, entity.y), (entity.x - y, entity.y + x));
    //         MarkLineOfSight((entity.x, entity.y), (entity.x + y, entity.y - x));
    //         MarkLineOfSight((entity.x, entity.y), (entity.x - y, entity.y - x));
    //     }
    // }

    // private bool MarkLineOfSight((int x, int y) from, (int x, int y) to)
    // {
    //     TileMap tileMap = GetParent<TileMap>();

    //     bool returnFalseNext = false;
    //     foreach ((int x, int y) in LineBetween(from, to))
    //     {
    //         if (returnFalseNext)
    //         {
    //             return false;
    //         }
    //         if (tileMap.GetCell(x, y) == -1)
    //         {
    //             returnFalseNext = true;
    //         }
    //         SetCell(x, y, VISIBLE);
    //     }
    //     return true;
    // }

    // Math Part

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


    private IEnumerable<(int numerator, int denominator)> ListRationals(int radius)
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

    private static int GCD(int a, int b)
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
}
