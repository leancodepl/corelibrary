/*
MIT License

Copyright (c) 2019 Cysharp, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace LeanCode.DomainModels.Ulids;

internal static class UlidRandomProvider
{
    [ThreadStatic]
    private static Random? random;

    [ThreadStatic]
    private static XorShift64? xorShift;

    // this random is async-unsafe, be careful to use.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Random GetRandom()
    {
        return random ??= CreateRandom();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Random CreateRandom()
    {
        using var rng = RandomNumberGenerator.Create();
        // Span<byte> buffer = stackalloc byte[sizeof(int)];
        var buffer = new byte[sizeof(int)];
        rng.GetBytes(buffer);
        var seed = BitConverter.ToInt32(buffer, 0);
        return new Random(seed);
    }

    // this random is async-unsafe, be careful to use.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XorShift64 GetXorShift64()
    {
        return xorShift ??= CreateXorShift64();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static XorShift64 CreateXorShift64()
    {
        using var rng = RandomNumberGenerator.Create();
        // Span<byte> buffer = stackalloc byte[sizeof(UInt64)];
        var buffer = new byte[sizeof(ulong)];
        rng.GetBytes(buffer);
        var seed = BitConverter.ToUInt64(buffer, 0);
        return new XorShift64(seed);
    }
}

internal class XorShift64
{
    private ulong x = 88172645463325252UL;

    public XorShift64(ulong seed)
    {
        if (seed != 0)
        {
            x = seed;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        x = x ^ (x << 7);
        return x = x ^ (x >> 9);
    }
}
