using System;

// More strict than Vector2i is with operators. Treat Vector2i as a RelativePosition.
public readonly struct AbsolutePosition
{
    // No constants defined intentionally.

    public readonly int x;
    public readonly int y;

    public AbsolutePosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    // This is the only operation between absolute positions that makes sense.
    // (to - from) is a delta. then from + (to - from) = to.
    public static Vector2i operator -(AbsolutePosition to, AbsolutePosition from)
    {
        return new Vector2i(to.x - from.x, to.y - from.y);
    }

    // Then we have relative position stuff.

    // Absolute positions always go on the left, for readability.
    public static AbsolutePosition operator +(AbsolutePosition pos, Vector2i delta)
    {
        return new AbsolutePosition(pos.x + delta.x, pos.y + delta.y);
    }

    public static AbsolutePosition operator -(AbsolutePosition pos, Vector2i delta)
    {
        return pos + (-delta);
    }

    public static bool operator ==(AbsolutePosition left, AbsolutePosition right)
    {
        return left.x == right.x && left.y == right.y;
    }

    public static bool operator !=(AbsolutePosition left, AbsolutePosition right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is AbsolutePosition other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        // See Vector2i.
        return x ^ (y << 16 | y >> 16);
    }

    // Downcasting. Safe but tuples are not recommended.
    public static implicit operator (int x, int y)(AbsolutePosition from)
    {
        return (from.x, from.y);
    }

    // Upcasting! Loss of meaning.
    // TODO: After migration, mark explicit and remove [Obsolete].
    [Obsolete]
    public static implicit operator AbsolutePosition((int x, int y) tuple)
    {
        return new AbsolutePosition(tuple.x, tuple.y);
    }
}