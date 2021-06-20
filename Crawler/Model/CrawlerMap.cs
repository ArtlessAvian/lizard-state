using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Stores map and vision information.
/// Maybe ask the 
/// </summary>
public class CrawlerMap : TileMap
{
    // hehe parasitic inheritance.
    // creates orphan nodes >:/
    public TileMap fog;

    private const int REVEALED = 0;
    private const int VISIBLE = 1;

    public CrawlerMap()
    {
        fog = new TileMap(); // unrevealed tiles are -1 by default.
    }

    // Assume this is always true.
    // Hopefully ez fix if its an issue.
    public static bool TileIsWall(int id)
    {
        return id == -1 || id == 6;
    }

    // Return value to be sent to ViewModel.
    // Radius should be a small reasonable number, like 6.
    public int[,] GetVisibleTiles((int x, int y) pos, int radius = 6)
    {
        ClearVisibility();
        UpdateVisibility(pos, radius);

        int[,] tiles = new int[radius * 2 + 1, radius * 2 + 1];
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                tiles[dx + radius, dy + radius] = 
                    fog.GetCell(pos.x + dx, pos.y + dy) == VISIBLE ?
                    this.GetCell(pos.x + dx, pos.y + dy) : -2;
                    // -2 and not -1, in case theres a hole in the ground or something
            }
        }
        return tiles;
    }

    private void ClearVisibility()
    {
        foreach (Vector2 vec in fog.GetUsedCellsById(VISIBLE))
        {
            fog.SetCellv(vec, REVEALED);
        }
    }

    // Tiles marked as VISIBLE are not meant to be saved!
    public void UpdateVisibility((int x, int y) pos, int radius)
    {
        // For each unique slope passing through a cell,
        foreach ((int x, int y) in ListRationals(radius))
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
            fog.SetCell(x, y, VISIBLE);
            if (TileIsWall(this.GetCell(x, y)))
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
