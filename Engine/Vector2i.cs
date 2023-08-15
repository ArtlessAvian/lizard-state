using System;

namespace LizardState.Engine
{
    // Much friendlier/meaningfuller than a tuple, (int x, int y).
    // Ideally is a drop-in replacement for Godot's Vector2i type. (I'm not sure if its more complex or just this simple.)
    // Not a resource. If you need Godot to serialize, make this a Property and export ints x and y.
    public readonly struct Vector2i
    {
        public static Vector2i ZERO = new Vector2i(0, 0);
        public static Vector2i ONE = new Vector2i(1, 1);
        public static Vector2i LEFT = new Vector2i(-1, 0);
        public static Vector2i RIGHT = new Vector2i(1, 0);
        public static Vector2i UP = new Vector2i(0, -1);
        public static Vector2i DOWN = new Vector2i(0, 1);

        public readonly int x;
        public readonly int y;

        // try to use the constructor, for that drop-in-ness.
        // the {} definitions are fine **internally only**.
        public Vector2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2i operator -(Vector2i val)
        {
            return new Vector2i(-val.x, -val.y);
        }

        public static Vector2i operator +(Vector2i left, Vector2i right)
        {
            return new Vector2i(left.x + right.x, left.y + right.y);
        }

        public static Vector2i operator -(Vector2i left, Vector2i right)
        {
            return left + (-right);
        }

        public static bool operator ==(Vector2i left, Vector2i right)
        {
            return left.x == right.x && left.y == right.y;
        }

        public static bool operator !=(Vector2i left, Vector2i right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2i other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            // x xor y circular shift.
            // Probably behaves poorly with twos compliment.
            // like, v collides with -v.
            return x ^ (y << 16 | y >> 16);
        }

        // Upcasting. Safe but tuples are not recommended.
        [Obsolete]
        public static implicit operator (int x, int y)(Vector2i from)
        {
            return (from.x, from.y);
        }

        // Downcasting! Not all tuples are positions! You should just use the constructor.
        [Obsolete]
        public static implicit operator Vector2i((int x, int y) tuple)
        {
            return new Vector2i(tuple.x, tuple.y);
        }
    }
}