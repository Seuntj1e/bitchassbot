using System;
using System.Collections;
using System.Collections.Generic;

namespace BitchAssBot.Models
{
    public class Position
    {
        public override string ToString()
        {
            return $"({X},{Y})";
        }
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Position()
        {
        }
        public Position checknext(int direction)
        {
            return new Position(this, direction);
        }

        public Position(Position old, int direction)
        {
            this.X = old.X;
            this.Y = old.Y;
            switch (direction)
            {
                case 1: X++; break;
                case 2: Y++; break;
                case 3: X--; break;
                case 4: Y--; break;
                case 5: X++; Y++; break;
                case 6: X++; Y--; break;
                case 7: X--; Y++; break;
                case 8: X--; Y--; break;
            }
        }

        protected bool Equals(Position other)
        {
            return X.Equals(other?.X) && Y.Equals(other?.Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Position) && obj.GetType() != typeof(Land)) return false;
            return Equals((Position) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }


        public static bool operator == (Position position1, Position position2)
        {
            return !(position1 is null) && position1.Equals(position2);
        }

        public static bool operator != (Position position1, Position position2)
        {
            return !(position1.Equals(position2));
        }
    }

    public class PositionComparer : IEqualityComparer<Position>
    {
        public bool Equals(Position p1, Position p2)
        {
            return !ReferenceEquals(p1, null) && p1.Equals(p2);
        }

        public int GetHashCode(Position obj)
        {
            return obj.GetHashCode();
        }
    }
}