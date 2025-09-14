using System;

namespace Nogue.Core
{
    // xoshiro128++ deterministic PRNG with hierarchical seeding
    // State: 4 x uint (128-bit)
    public sealed class Xoshiro128PlusPlus
    {
        private uint s0, s1, s2, s3;

        public Xoshiro128PlusPlus(ulong seed)
        {
            // Seed via splitmix64 -> 128-bit state
            ulong x = seed;
            s0 = NextSplitMix(ref x);
            s1 = NextSplitMix(ref x);
            s2 = NextSplitMix(ref x);
            s3 = NextSplitMix(ref x);
            if ((s0 | s1 | s2 | s3) == 0) s0 = 0x9E3779B9u; // avoid all-zero
        }

        public Xoshiro128PlusPlus(uint a, uint b, uint c, uint d)
        { s0 = a; s1 = b; s2 = c; s3 = d; }

        private static uint NextSplitMix(ref ulong x)
        {
            x += 0x9E3779B97F4A7C15UL;
            ulong z = x;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            z ^= (z >> 31);
            return (uint)z;
        }

        private static uint RotL(uint x, int k) => (x << k) | (x >> (32 - k));

        // Next 32-bit unsigned int
        public uint NextUInt32()
        {
            uint result = RotL(s0 + s3, 7) + s0;
            uint t = s1 << 9;

            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;

            s2 ^= t;
            s3 = RotL(s3, 11);

            return result;
        }

        // [0,1)
        public double NextDouble()
        {
            // 53-bit precision via two draws
            ulong a = NextUInt32();
            ulong b = NextUInt32();
            ulong bits = (a << 21) ^ b;
            return (bits & ((1UL << 53) - 1)) / (double)(1UL << 53);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            uint range = (uint)(maxExclusive - minInclusive);
            return (int)(NextUInt32() % range) + minInclusive;
        }

        public bool Chance(double p) => NextDouble() < p;

        // Hierarchical derivation: mix with label to produce child RNG
        public Xoshiro128PlusPlus Derive(ReadOnlySpan<char> label)
        {
            // Simple 64-bit FNV-1a over (state||label)
            ulong hash = 1469598103934665603UL; // FNV offset basis
            void Mix(uint v)
            {
                hash ^= v;
                hash *= 1099511628211UL;
            }
            Mix(s0); Mix(s1); Mix(s2); Mix(s3);
            foreach (char c in label) { Mix((uint)c); }
            return new Xoshiro128PlusPlus(hash);
        }
    }
}

