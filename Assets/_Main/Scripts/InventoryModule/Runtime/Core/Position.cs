using System;

namespace InventoryModule
{
    [Serializable]
    public class Position
    {
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position other)
                return X == other.X && Y == other.Y;

            return false;
        }

        public override int GetHashCode() => X.GetHashCode() * 31 + Y.GetHashCode();

        public override string ToString() => $"({X}, {Y})";
    }
}