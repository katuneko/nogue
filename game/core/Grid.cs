using System;
using System.Collections.Generic;

namespace Nogue.Core
{
    // 2D integer coordinate on the tile grid (1 tile = 1m)
    public readonly struct Coord : IEquatable<Coord>
    {
        public readonly int X;
        public readonly int Y;

        public Coord(int x, int y)
        {
            X = x; Y = y;
        }

        public Coord Add(int dx, int dy) => new Coord(X + dx, Y + dy);

        public bool Equals(Coord other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is Coord c && Equals(c);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X},{Y})";

        // Manhattan neighbors (4-neighborhood, r=1)
        public IEnumerable<Coord> Neighbors4()
        {
            yield return new Coord(X + 1, Y);
            yield return new Coord(X - 1, Y);
            yield return new Coord(X, Y + 1);
            yield return new Coord(X, Y - 1);
        }

        // Moore neighborhood (8-neighborhood, r=1)
        public IEnumerable<Coord> Neighbors8()
        {
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                yield return new Coord(X + dx, Y + dy);
            }
        }
    }

    // Patch = 16x16 tiles block
    public readonly struct PatchId : IEquatable<PatchId>
    {
        public readonly int Value;
        public PatchId(int value) { Value = value; }
        public bool Equals(PatchId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is PatchId p && Equals(p);
        public override int GetHashCode() => Value;
        public override string ToString() => $"Patch#{Value}";
    }
}

