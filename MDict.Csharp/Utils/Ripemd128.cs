/*
 * A pure JavaScript implementation of RIPEMD128 using Uint8Array as input/output.
 *
 * Based on coiscir/jsdigest (https://github.com/coiscir/jsdigest/blob/master/src/hash/ripemd128.js)
 *
 * ripemd128.js is free software released under terms of the MIT License.
 * You can get a copy on http://opensource.org/licenses/MIT.
 *
 *
 * RIPEMD-128 (c) 1996 Hans Dobbertin, Antoon Bosselaers, and Bart Preneel
 */

namespace MDict.Csharp.Utils;

/// <summary>
/// Example usage:
/// <code>
/// byte[] input = System.Text.Encoding.UTF8.GetBytes("abc");
/// byte[] digest = RIPEMD128.ComputeHash(input);
/// Console.WriteLine(BitConverter.ToString(digest).Replace("-", "").ToLower());
/// </code>
/// </summary>
internal static class Ripemd128
{
    // private const uint DIGEST = 128;
    // private const uint BLOCK = 64;

    private static readonly uint[][] S = new uint[][]
    {
        new uint[] {11, 14, 15, 12, 5, 8, 7, 9, 11, 13, 14, 15, 6, 7, 9, 8}, // round 1
        new uint[] {7, 6, 8, 13, 11, 9, 7, 15, 7, 12, 15, 9, 11, 7, 13, 12}, // round 2
        new uint[] {11, 13, 6, 7, 14, 9, 13, 15, 14, 8, 13, 6, 5, 12, 7, 5}, // round 3
        new uint[] {11, 12, 14, 15, 14, 15, 9, 8, 9, 14, 5, 6, 8, 6, 5, 12}, // round 4
        new uint[] {8, 9, 9, 11, 13, 15, 15, 5, 7, 7, 8, 11, 14, 14, 12, 6}, // parallel round 1
        new uint[] {9, 13, 15, 7, 12, 8, 9, 11, 7, 7, 12, 7, 6, 15, 13, 11}, // parallel round 2
        new uint[] {9, 7, 15, 11, 8, 6, 6, 14, 12, 13, 5, 14, 13, 13, 7, 5}, // parallel round 3
        new uint[] {15, 5, 8, 11, 14, 14, 6, 14, 6, 9, 12, 9, 12, 5, 15, 8}, // parallel round 4
    };

    private static readonly uint[][] X = new uint[][]
    {
        new uint[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15},
        new uint[] {7, 4, 13, 1, 10, 6, 15, 3, 12, 0, 9, 5, 2, 14, 11, 8},
        new uint[] {3, 10, 14, 4, 9, 15, 8, 1, 2, 7, 0, 6, 13, 11, 5, 12},
        new uint[] {1, 9, 11, 10, 0, 8, 12, 4, 13, 3, 7, 15, 14, 5, 6, 2},
        new uint[] {5, 14, 7, 0, 9, 2, 11, 4, 13, 6, 15, 8, 1, 10, 3, 12},
        new uint[] {6, 11, 3, 7, 0, 13, 5, 10, 14, 15, 8, 12, 4, 9, 1, 2},
        new uint[] {15, 5, 1, 3, 7, 14, 6, 9, 11, 8, 12, 2, 10, 0, 4, 13},
        new uint[] {8, 6, 4, 1, 3, 11, 15, 0, 5, 12, 2, 13, 9, 7, 10, 14},
    };

    private static readonly uint[] K = new uint[]
    {
        0x00000000, // FF
        0x5a827999, // GG
        0x6ed9eba1, // HH
        0x8f1bbcdc, // II
        0x50a28be6, // III
        0x5c4dd124, // HHH
        0x6d703ef3, // GGG
        0x00000000, // FFF
    };

    private static readonly Func<uint, uint, uint, uint>[] F = new Func<uint, uint, uint, uint>[]
    {
        (x, y, z) => x ^ y ^ z,
        (x, y, z) => x & y | ~x & z,
        (x, y, z) => (x | ~y) ^ z,
        (x, y, z) => x & z | y & ~z,
    };

    /// <summary>
    /// Swap high and low bits of a 32-bit int.
    /// </summary>
    private static uint Rotl(uint x, int n)
    {
        return x << n | x >> 32 - n;
    }

    /// <summary>
    /// Concat 2 typed array.
    /// </summary>
    private static byte[] Concat(byte[] a, byte[] b)
    {
        if ((a == null || a.Length == 0) && (b == null || b.Length == 0))
            throw new ArgumentException("Invalid Buffer a and b");
        if (a == null || a.Length == 0) return b;
        if (b == null || b.Length == 0) return a;

        var c = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, c, 0, a.Length);
        Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
        return c;
    }

    public static byte[] ComputeHash(byte[] input)
    {
        uint aa, bb, cc, dd, aaa, bbb, ccc, ddd, tmp;
        int t, r, rr;

        uint[] hash = new uint[]
        {
            0x67452301, 0xefcdab89, 0x98badcfe, 0x10325476
        };

        int bytes = input.Length;
        var padding = new byte[(bytes % 64 < 56 ? 56 : 120) - bytes % 64];
        padding[0] = 0x80;

        byte[] dataWithPad = Concat(input, padding);

        // append length as 64-bit little-endian
        byte[] lengthBytes = new byte[8];
        BitConverter.GetBytes((long)bytes * 8).CopyTo(lengthBytes, 0);

        byte[] fullData = Concat(dataWithPad, lengthBytes);
        uint[] x = new uint[fullData.Length / 4];
        Buffer.BlockCopy(fullData, 0, x, 0, fullData.Length);

        // Update hash
        for (int i = 0; i < x.Length; i += 16)
        {
            aa = aaa = hash[0];
            bb = bbb = hash[1];
            cc = ccc = hash[2];
            dd = ddd = hash[3];

            for (t = 0; t < 64; t++)
            {
                r = t / 16;
                aa = Rotl(aa + F[r](bb, cc, dd) + x[i + X[r][t % 16]] + K[r], (int)S[r][t % 16]);
                tmp = dd; dd = cc; cc = bb; bb = aa; aa = tmp;
            }

            for (; t < 128; t++)
            {
                r = t / 16;
                rr = (63 - t % 64) / 16;
                aaa = Rotl(aaa + F[rr](bbb, ccc, ddd) + x[i + X[r][t % 16]] + K[r], (int)S[r][t % 16]);
                tmp = ddd; ddd = ccc; ccc = bbb; bbb = aaa; aaa = tmp;
            }

            ddd = hash[1] + cc + ddd;
            hash[1] = hash[2] + dd + aaa;
            hash[2] = hash[3] + aa + bbb;
            hash[3] = hash[0] + bb + ccc;
            hash[0] = ddd;
        }

        byte[] result = new byte[16];
        Buffer.BlockCopy(hash, 0, result, 0, result.Length);
        return result;
    }
}
