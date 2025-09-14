using System;

namespace Nogue.Core
{
    // Q16.16 fixed point number backed by int
    public readonly struct Fixed16 : IComparable<Fixed16>, IEquatable<Fixed16>
    {
        public readonly int Raw;
        public const int FRACTION_BITS = 16;
        public const int ONE = 1 << FRACTION_BITS;

        public Fixed16(int raw) { Raw = raw; }

        public static Fixed16 FromFloat(float f) => new Fixed16((int)MathF.Round(f * ONE));
        public static Fixed16 FromDouble(double d) => new Fixed16((int)Math.Round(d * ONE));
        public float ToFloat() => Raw / (float)ONE;
        public double ToDouble() => Raw / (double)ONE;

        public static Fixed16 operator +(Fixed16 a, Fixed16 b) => new Fixed16(a.Raw + b.Raw);
        public static Fixed16 operator -(Fixed16 a, Fixed16 b) => new Fixed16(a.Raw - b.Raw);
        public static Fixed16 operator *(Fixed16 a, Fixed16 b) => new Fixed16((int)(((long)a.Raw * b.Raw) >> FRACTION_BITS));
        public static Fixed16 operator /(Fixed16 a, Fixed16 b) => new Fixed16((int)(((long)a.Raw << FRACTION_BITS) / b.Raw));

        public int CompareTo(Fixed16 other) => Raw.CompareTo(other.Raw);
        public bool Equals(Fixed16 other) => Raw == other.Raw;
        public override bool Equals(object? obj) => obj is Fixed16 f && Equals(f);
        public override int GetHashCode() => Raw;
        public override string ToString() => ToDouble().ToString("0.####");

        public static readonly Fixed16 Zero = new Fixed16(0);
        public static readonly Fixed16 One = new Fixed16(ONE);
    }
}

