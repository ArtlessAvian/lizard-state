using System;

// More nerd math. Two sectors of squares with the same "squangles" have the same area.
// The same cannot be done with regular trig. 
static class SquareSectorHelper
{
    public const float FULL_REVOLUTION = 8;

    public static float PointToAngle(float x, float y)
    {
        if (x == 0 && y == 0) { return float.NaN; }
        if (y < 0) { return -PointToAngle(x, -y); }
        if (x < 0) { return 4 - PointToAngle(-x, y); }
        if (x < y) { return 2 - PointToAngle(y, x); }
        // now, 0 < y < x.
        return y / x;
    }

    public static (float x, float y) AngleToPoint(float squradians)
    {
        if (squradians < 0)
        {
            (float x, float y) = AngleToPoint(-squradians);
            return (x, -y);
        }

        float thingy = (squradians + 3) % 4;
        if (thingy < 2)
        {
            if (squradians % 8 < 4) { return (1 - (squradians + 7) % 8, 1); }
            else { return ((squradians + 3) % 8 - 1, -1); }
        }
        else
        {
            // if ((squradians + 6) % 8 < 4) { return (1, y); }
            // else { return (-1, y); }
            // rotate, calculate, unrotate answer.
            (float x, float y) = AngleToPoint(squradians + 6);
            return (-y, x);
        }
    }

    public static float AngleBetween(float x, float y, float x2, float y2)
    {
        float diff = Math.Abs(PointToAngle(x, y) - PointToAngle(x2, y2));
        if (diff > 4) { return 8 - diff; }
        return diff;
    }

    public static (float x, float y) ProjectUnitSquare(float x, float y)
    {
        if (x == 0 && y == 0) { return (0, 0); }
        if (x == 0) { return (0, Math.Sign(y)); }
        if (y == 0) { return (Math.Sign(x), 0); }

        if (Math.Abs(x) <= Math.Abs(y))
        {
            return (Math.Sign(x), y / Math.Abs(x));
        }
        return (x / Math.Abs(y), Math.Sign(y));
    }
}