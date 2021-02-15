using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class Map
{
    private int visionRadius;

    // hehe parasitic inheritance.
    public TileMap map;
    public TileMap visibility;

    private const int VISIBLE = 1;
    private const int REVEALED = 0;

    public Map(int visionRadius = 6)
    {
        this.visionRadius = visionRadius;

        map = new TileMap();
        visibility = new TileMap();
    }

    // Sent to ViewModel.
    public int[,] GetVisibleTiles((int x, int y) pos)
    {
        int[,] tiles = new int[visionRadius * 2 + 1, visionRadius * 2 + 1];
        for (int dy = -visionRadius; dy <= visionRadius; dy++)
        {
            for (int dx = -visionRadius; dx <= visionRadius; dx++)
            {
                tiles[dx + visionRadius, dy + visionRadius] = 
                    visibility.GetCell(pos.x + dx, pos.y + dy) == VISIBLE ?
                    map.GetCell(pos.x + dx, pos.y + dy) : -1;
            }
        }
        return tiles;
    }

    public void ClearVisibility()
    {
        foreach (Vector2 vec in visibility.GetUsedCellsById(VISIBLE))
        {
            visibility.SetCellv(vec, REVEALED);
        }
    }

    public void UpdateVisibility((int x, int y) pos)
    {
        // For each unique slope passing through a cell,
        foreach ((int x, int y) in ListRationals(visionRadius))
        {
            // Mark every cell on that slope, for each of the 8 octants.
            MarkLineOfSight((pos.x, pos.y), (pos.x + x, pos.y + y));
            MarkLineOfSight((pos.x, pos.y), (pos.x - x, pos.y + y));
            MarkLineOfSight((pos.x, pos.y), (pos.x + x, pos.y - y));
            MarkLineOfSight((pos.x, pos.y), (pos.x - x, pos.y - y));
            MarkLineOfSight((pos.x, pos.y), (pos.x + y, pos.y + x));
            MarkLineOfSight((pos.x, pos.y), (pos.x - y, pos.y + x));
            MarkLineOfSight((pos.x, pos.y), (pos.x + y, pos.y - x));
            MarkLineOfSight((pos.x, pos.y), (pos.x - y, pos.y - x));
        }
    }

    private void MarkLineOfSight((int x, int y) from, (int x, int y) to)
    {
        foreach ((int x, int y) in LineBetween(from, to))
        {
            visibility.SetCell(x, y, VISIBLE);
            if (map.GetCell(x, y) == -1)
            {
                return;
            }
        }
        return;
    }

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
