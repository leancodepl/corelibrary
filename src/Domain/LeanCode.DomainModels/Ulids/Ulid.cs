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

using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace LeanCode.DomainModels.Ulids;

/// <summary>
/// Represents a Universally Unique Lexicographically Sortable Identifier (ULID).
/// Spec: https://github.com/ulid/spec
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 16)]
[DebuggerDisplay("{ToString(),nq}")]
[System.Text.Json.Serialization.JsonConverter(typeof(UlidJsonConverter))]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public readonly record struct Ulid
    : IEquatable<Ulid>,
        IComparable<Ulid>,
        ISpanFormattable,
        ISpanParsable<Ulid>,
        IUtf8SpanFormattable
{
    public const int LengthInTextElements = 26;

    // https://en.wikipedia.org/wiki/Base32
    private static readonly ImmutableArray<char> Base32Text = "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToImmutableArray();
    private static readonly ImmutableArray<byte> Base32Bytes = Encoding.UTF8
        .GetBytes(Base32Text.ToArray())
        .ToImmutableArray();

    private static readonly ImmutableArray<byte> CharToBase32 = new byte[]
    {
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8,
        9,
        255,
        255,
        255,
        255,
        255,
        255,
        255,
        10,
        11,
        12,
        13,
        14,
        15,
        16,
        17,
        255,
        18,
        19,
        255,
        20,
        21,
        255,
        22,
        23,
        24,
        25,
        26,
        255,
        27,
        28,
        29,
        30,
        31,
        255,
        255,
        255,
        255,
        255,
        255,
        10,
        11,
        12,
        13,
        14,
        15,
        16,
        17,
        255,
        18,
        19,
        255,
        20,
        21,
        255,
        22,
        23,
        24,
        25,
        26,
        255,
        27,
        28,
        29,
        30,
        31
    }.ToImmutableArray();

    public static readonly Ulid MinValue = new Ulid(
        DateTimeOffset.UnixEpoch.ToUnixTimeMilliseconds(),
        new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
    );

    public static readonly Ulid MaxValue = new Ulid(
        DateTimeOffset.MaxValue.ToUnixTimeMilliseconds(),
        new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 }
    );

    public static readonly Ulid Empty;

    // Core

    // Timestamp(48bits)
    [FieldOffset(0)]
    private readonly byte timestamp0;

    [FieldOffset(1)]
    private readonly byte timestamp1;

    [FieldOffset(2)]
    private readonly byte timestamp2;

    [FieldOffset(3)]
    private readonly byte timestamp3;

    [FieldOffset(4)]
    private readonly byte timestamp4;

    [FieldOffset(5)]
    private readonly byte timestamp5;

    // Randomness(80bits)
    [FieldOffset(6)]
    private readonly byte randomness0;

    [FieldOffset(7)]
    private readonly byte randomness1;

    [FieldOffset(8)]
    private readonly byte randomness2;

    [FieldOffset(9)]
    private readonly byte randomness3;

    [FieldOffset(10)]
    private readonly byte randomness4;

    [FieldOffset(11)]
    private readonly byte randomness5;

    [FieldOffset(12)]
    private readonly byte randomness6;

    [FieldOffset(13)]
    private readonly byte randomness7;

    [FieldOffset(14)]
    private readonly byte randomness8;

    [FieldOffset(15)]
    private readonly byte randomness9;

    [IgnoreDataMember]
    [SuppressMessage("?", "CA1819", Justification = "Fresh array is allocated per call")]
    public byte[] Random =>
        new byte[]
        {
            randomness0,
            randomness1,
            randomness2,
            randomness3,
            randomness4,
            randomness5,
            randomness6,
            randomness7,
            randomness8,
            randomness9,
        };

    [IgnoreDataMember]
    public DateTimeOffset Time
    {
        get
        {
            Span<byte> buffer = stackalloc byte[8];
            buffer[0] = timestamp5;
            buffer[1] = timestamp4;
            buffer[2] = timestamp3;
            buffer[3] = timestamp2;
            buffer[4] = timestamp1;
            buffer[5] = timestamp0; // [6], [7] = 0

            var ts = BinaryPrimitives.ReadInt64LittleEndian(buffer);
            return DateTimeOffset.FromUnixTimeMilliseconds(ts);
        }
    }

    internal Ulid(long timestampMilliseconds, XorShift64 random)
        : this()
    {
        WriteTimestamp(ref this, timestampMilliseconds);

        // Get first byte of randomness from Ulid Struct.
        Unsafe.WriteUnaligned(ref randomness0, random.Next()); // randomness0~7(but use 0~1 only)
        Unsafe.WriteUnaligned(ref randomness2, random.Next()); // randomness2~9
    }

    internal Ulid(long timestampMilliseconds, ReadOnlySpan<byte> randomness)
        : this()
    {
        WriteTimestamp(ref this, timestampMilliseconds);

        ref var src = ref MemoryMarshal.GetReference(randomness); // length = 10
        randomness0 = randomness[0];
        randomness1 = randomness[1];
        Unsafe.WriteUnaligned(ref randomness2, Unsafe.As<byte, ulong>(ref Unsafe.Add(ref src, 2))); // randomness2~randomness9
    }

    public Ulid(ReadOnlySpan<byte> bytes)
        : this()
    {
        if (bytes.Length != 16)
        {
            throw new ArgumentException("invalid bytes length, length:" + bytes.Length);
        }

        ref var src = ref MemoryMarshal.GetReference(bytes);
        Unsafe.WriteUnaligned(ref timestamp0, Unsafe.As<byte, ulong>(ref src)); // timestamp0~randomness1
        Unsafe.WriteUnaligned(ref randomness2, Unsafe.As<byte, ulong>(ref Unsafe.Add(ref src, 8))); // randomness2~randomness9
    }

    internal Ulid(ReadOnlySpan<char> base32)
    {
        // unroll-code is based on NUlid.

        randomness9 = (byte)((CharToBase32[base32[24]] << 5) | CharToBase32[base32[25]]); // eliminate bounds-check of span

        timestamp0 = (byte)((CharToBase32[base32[0]] << 5) | CharToBase32[base32[1]]);
        timestamp1 = (byte)((CharToBase32[base32[2]] << 3) | (CharToBase32[base32[3]] >> 2));
        timestamp2 = (byte)(
            (CharToBase32[base32[3]] << 6) | (CharToBase32[base32[4]] << 1) | (CharToBase32[base32[5]] >> 4)
        );
        timestamp3 = (byte)((CharToBase32[base32[5]] << 4) | (CharToBase32[base32[6]] >> 1));
        timestamp4 = (byte)(
            (CharToBase32[base32[6]] << 7) | (CharToBase32[base32[7]] << 2) | (CharToBase32[base32[8]] >> 3)
        );
        timestamp5 = (byte)((CharToBase32[base32[8]] << 5) | CharToBase32[base32[9]]);

        randomness0 = (byte)((CharToBase32[base32[10]] << 3) | (CharToBase32[base32[11]] >> 2));
        randomness1 = (byte)(
            (CharToBase32[base32[11]] << 6) | (CharToBase32[base32[12]] << 1) | (CharToBase32[base32[13]] >> 4)
        );
        randomness2 = (byte)((CharToBase32[base32[13]] << 4) | (CharToBase32[base32[14]] >> 1));
        randomness3 = (byte)(
            (CharToBase32[base32[14]] << 7) | (CharToBase32[base32[15]] << 2) | (CharToBase32[base32[16]] >> 3)
        );
        randomness4 = (byte)((CharToBase32[base32[16]] << 5) | CharToBase32[base32[17]]);
        randomness5 = (byte)((CharToBase32[base32[18]] << 3) | CharToBase32[base32[19]] >> 2);
        randomness6 = (byte)(
            (CharToBase32[base32[19]] << 6) | (CharToBase32[base32[20]] << 1) | (CharToBase32[base32[21]] >> 4)
        );
        randomness7 = (byte)((CharToBase32[base32[21]] << 4) | (CharToBase32[base32[22]] >> 1));
        randomness8 = (byte)(
            (CharToBase32[base32[22]] << 7) | (CharToBase32[base32[23]] << 2) | (CharToBase32[base32[24]] >> 3)
        );
    }

    // HACK: We assume the layout of a Guid is the following:
    // Int32, Int16, Int16, Int8, Int8, Int8, Int8, Int8, Int8, Int8, Int8
    // source: https://github.com/dotnet/runtime/blob/4f9ae42d861fcb4be2fcd5d3d55d5f227d30e723/src/libraries/System.Private.CoreLib/src/System/Guid.cs
    public Ulid(Guid guid)
    {
        Span<byte> buf = stackalloc byte[16];
        MemoryMarshal.Write(buf, ref guid);
        if (BitConverter.IsLittleEndian)
        {
            // Align with Guid layout - Int32, Int16, Int16, 8x Int8

            buf[0..4].Reverse();
            buf[4..6].Reverse();
            buf[6..8].Reverse();
        }
        this = MemoryMarshal.Read<Ulid>(buf);
    }

    private static void WriteTimestamp(ref Ulid ulid, long timestampMilliseconds)
    {
        if (BitConverter.IsLittleEndian)
        {
            timestampMilliseconds = BinaryPrimitives.ReverseEndianness(timestampMilliseconds);
        }

        var sourceSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref timestampMilliseconds, 1))[2..]; // Ignore two oldest bytes
        var destSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref ulid, 1));

        sourceSpan.CopyTo(destSpan);
    }

    // Factory

    public static Ulid NewUlid()
    {
        return new Ulid(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), UlidRandomProvider.GetXorShift64());
    }

    public static Ulid NewUlid(DateTimeOffset timestamp)
    {
        return new Ulid(timestamp.ToUnixTimeMilliseconds(), UlidRandomProvider.GetXorShift64());
    }

    public static Ulid NewUlid(DateTimeOffset timestamp, ReadOnlySpan<byte> randomness)
    {
        if (randomness.Length != 10)
        {
            throw new ArgumentException("invalid randomness length, length:" + randomness.Length);
        }

        return new Ulid(timestamp.ToUnixTimeMilliseconds(), randomness);
    }

    [SuppressMessage("?", "CA1305", Justification = "Ulids ignore user culture")]
    public static Ulid Parse(string base32)
    {
        return Parse(base32.AsSpan());
    }

    public static Ulid Parse(ReadOnlySpan<char> base32)
    {
        if (base32.Length != LengthInTextElements)
        {
            throw new ArgumentException("invalid base32 length, length:" + base32.Length);
        }

        return new Ulid(base32);
    }

    public static Ulid Parse(ReadOnlySpan<byte> base32)
    {
        if (!TryParse(base32, out var ulid))
        {
            throw new ArgumentException("invalid base32 length, length:" + base32.Length);
        }

        return ulid;
    }

    public static bool TryParse([NotNullWhen(true)] string? base32, out Ulid ulid)
    {
        return TryParse(base32.AsSpan(), out ulid);
    }

    [SuppressMessage("?", "CA1031", Justification = "Method is an exception boundary")]
    public static bool TryParse(ReadOnlySpan<char> base32, out Ulid ulid)
    {
        if (base32.Length != LengthInTextElements)
        {
            ulid = default(Ulid);
            return false;
        }

        try
        {
            ulid = new Ulid(base32);
            return true;
        }
        catch
        {
            ulid = default(Ulid);
            return false;
        }
    }

    [SuppressMessage("?", "CA1031", Justification = "Method is an exception boundary")]
    public static bool TryParse(ReadOnlySpan<byte> base32, out Ulid ulid)
    {
        if (base32.Length != LengthInTextElements)
        {
            ulid = default(Ulid);
            return false;
        }

        try
        {
            ulid = ParseCore(base32);
            return true;
        }
        catch
        {
            ulid = default(Ulid);
            return false;
        }
    }

    [SuppressMessage("?", "CA1305", Justification = "Ulids ignore user culture")]
    public static Ulid Parse(string s, IFormatProvider? provider) => Parse(s);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Ulid result) =>
        TryParse(s, out result);

    [SuppressMessage("?", "CA1305", Justification = "Ulids ignore user culture")]
    public static Ulid Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Ulid result) =>
        TryParse(s, out result);

    private static Ulid ParseCore(ReadOnlySpan<byte> base32)
    {
        if (base32.Length != LengthInTextElements)
        {
            throw new ArgumentException("invalid base32 length, length:" + base32.Length);
        }

        var ulid = default(Ulid);

        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 15) = (byte)(
            (CharToBase32[base32[24]] << 5) | CharToBase32[base32[25]]
        );

        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 0) = (byte)(
            (CharToBase32[base32[0]] << 5) | CharToBase32[base32[1]]
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 1) = (byte)(
            (CharToBase32[base32[2]] << 3) | (CharToBase32[base32[3]] >> 2)
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 2) = (byte)(
            (CharToBase32[base32[3]] << 6) | (CharToBase32[base32[4]] << 1) | (CharToBase32[base32[5]] >> 4)
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 3) = (byte)(
            (CharToBase32[base32[5]] << 4) | (CharToBase32[base32[6]] >> 1)
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 4) = (byte)(
            (CharToBase32[base32[6]] << 7) | (CharToBase32[base32[7]] << 2) | (CharToBase32[base32[8]] >> 3)
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 5) = (byte)(
            (CharToBase32[base32[8]] << 5) | CharToBase32[base32[9]]
        );

        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 6) = (byte)(
            (CharToBase32[base32[10]] << 3) | (CharToBase32[base32[11]] >> 2)
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 7) = (byte)(
            (CharToBase32[base32[11]] << 6) | (CharToBase32[base32[12]] << 1) | (CharToBase32[base32[13]] >> 4)
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 8) = (byte)(
            (CharToBase32[base32[13]] << 4) | (CharToBase32[base32[14]] >> 1)
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 9) = (byte)(
            (CharToBase32[base32[14]] << 7) | (CharToBase32[base32[15]] << 2) | (CharToBase32[base32[16]] >> 3)
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 10) = (byte)(
            (CharToBase32[base32[16]] << 5) | CharToBase32[base32[17]]
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 11) = (byte)(
            (CharToBase32[base32[18]] << 3) | CharToBase32[base32[19]] >> 2
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 12) = (byte)(
            (CharToBase32[base32[19]] << 6) | (CharToBase32[base32[20]] << 1) | (CharToBase32[base32[21]] >> 4)
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 13) = (byte)(
            (CharToBase32[base32[21]] << 4) | (CharToBase32[base32[22]] >> 1)
        );
        Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 14) = (byte)(
            (CharToBase32[base32[22]] << 7) | (CharToBase32[base32[23]] << 2) | (CharToBase32[base32[24]] >> 3)
        );

        return ulid;
    }

    // Convert

    public byte[] ToByteArray()
    {
        var bytes = new byte[16];
        Unsafe.WriteUnaligned(ref bytes[0], this);
        return bytes;
    }

    public bool TryWriteBytes(Span<byte> destination)
    {
        if (destination.Length < 16)
        {
            return false;
        }

        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), this);
        return true;
    }

    public string ToBase64(Base64FormattingOptions options = Base64FormattingOptions.None)
    {
        Span<byte> buffer = stackalloc byte[16];
        TryWriteBytes(buffer);
        return Convert.ToBase64String(buffer, options);
    }

    public override string ToString()
    {
        return string.Create(LengthInTextElements, this, (span, ulid) => ulid.TryFormat(span, out _, "", null));
    }

    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
    {
        if (destination.Length < LengthInTextElements)
        {
            charsWritten = 0;
            return false;
        }

        destination[25] = Base32Text[randomness9 & 31]; // eliminate bounds-check of span

        // timestamp
        destination[0] = Base32Text[(timestamp0 & 224) >> 5];
        destination[1] = Base32Text[timestamp0 & 31];
        destination[2] = Base32Text[(timestamp1 & 248) >> 3];
        destination[3] = Base32Text[((timestamp1 & 7) << 2) | ((timestamp2 & 192) >> 6)];
        destination[4] = Base32Text[(timestamp2 & 62) >> 1];
        destination[5] = Base32Text[((timestamp2 & 1) << 4) | ((timestamp3 & 240) >> 4)];
        destination[6] = Base32Text[((timestamp3 & 15) << 1) | ((timestamp4 & 128) >> 7)];
        destination[7] = Base32Text[(timestamp4 & 124) >> 2];
        destination[8] = Base32Text[((timestamp4 & 3) << 3) | ((timestamp5 & 224) >> 5)];
        destination[9] = Base32Text[timestamp5 & 31];

        // randomness
        destination[10] = Base32Text[(randomness0 & 248) >> 3];
        destination[11] = Base32Text[((randomness0 & 7) << 2) | ((randomness1 & 192) >> 6)];
        destination[12] = Base32Text[(randomness1 & 62) >> 1];
        destination[13] = Base32Text[((randomness1 & 1) << 4) | ((randomness2 & 240) >> 4)];
        destination[14] = Base32Text[((randomness2 & 15) << 1) | ((randomness3 & 128) >> 7)];
        destination[15] = Base32Text[(randomness3 & 124) >> 2];
        destination[16] = Base32Text[((randomness3 & 3) << 3) | ((randomness4 & 224) >> 5)];
        destination[17] = Base32Text[randomness4 & 31];
        destination[18] = Base32Text[(randomness5 & 248) >> 3];
        destination[19] = Base32Text[((randomness5 & 7) << 2) | ((randomness6 & 192) >> 6)];
        destination[20] = Base32Text[(randomness6 & 62) >> 1];
        destination[21] = Base32Text[((randomness6 & 1) << 4) | ((randomness7 & 240) >> 4)];
        destination[22] = Base32Text[((randomness7 & 15) << 1) | ((randomness8 & 128) >> 7)];
        destination[23] = Base32Text[(randomness8 & 124) >> 2];
        destination[24] = Base32Text[((randomness8 & 3) << 3) | ((randomness9 & 224) >> 5)];

        charsWritten = LengthInTextElements;
        return true;
    }

    public bool TryFormat(
        Span<byte> utf8Destination,
        out int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
    {
        if (utf8Destination.Length < LengthInTextElements)
        {
            bytesWritten = 0;
            return false;
        }

        utf8Destination[25] = Base32Bytes[randomness9 & 31]; // eliminate bounds-check of span

        // timestamp
        utf8Destination[0] = Base32Bytes[(timestamp0 & 224) >> 5];
        utf8Destination[1] = Base32Bytes[timestamp0 & 31];
        utf8Destination[2] = Base32Bytes[(timestamp1 & 248) >> 3];
        utf8Destination[3] = Base32Bytes[((timestamp1 & 7) << 2) | ((timestamp2 & 192) >> 6)];
        utf8Destination[4] = Base32Bytes[(timestamp2 & 62) >> 1];
        utf8Destination[5] = Base32Bytes[((timestamp2 & 1) << 4) | ((timestamp3 & 240) >> 4)];
        utf8Destination[6] = Base32Bytes[((timestamp3 & 15) << 1) | ((timestamp4 & 128) >> 7)];
        utf8Destination[7] = Base32Bytes[(timestamp4 & 124) >> 2];
        utf8Destination[8] = Base32Bytes[((timestamp4 & 3) << 3) | ((timestamp5 & 224) >> 5)];
        utf8Destination[9] = Base32Bytes[timestamp5 & 31];

        // randomness
        utf8Destination[10] = Base32Bytes[(randomness0 & 248) >> 3];
        utf8Destination[11] = Base32Bytes[((randomness0 & 7) << 2) | ((randomness1 & 192) >> 6)];
        utf8Destination[12] = Base32Bytes[(randomness1 & 62) >> 1];
        utf8Destination[13] = Base32Bytes[((randomness1 & 1) << 4) | ((randomness2 & 240) >> 4)];
        utf8Destination[14] = Base32Bytes[((randomness2 & 15) << 1) | ((randomness3 & 128) >> 7)];
        utf8Destination[15] = Base32Bytes[(randomness3 & 124) >> 2];
        utf8Destination[16] = Base32Bytes[((randomness3 & 3) << 3) | ((randomness4 & 224) >> 5)];
        utf8Destination[17] = Base32Bytes[randomness4 & 31];
        utf8Destination[18] = Base32Bytes[(randomness5 & 248) >> 3];
        utf8Destination[19] = Base32Bytes[((randomness5 & 7) << 2) | ((randomness6 & 192) >> 6)];
        utf8Destination[20] = Base32Bytes[(randomness6 & 62) >> 1];
        utf8Destination[21] = Base32Bytes[((randomness6 & 1) << 4) | ((randomness7 & 240) >> 4)];
        utf8Destination[22] = Base32Bytes[((randomness7 & 15) << 1) | ((randomness8 & 128) >> 7)];
        utf8Destination[23] = Base32Bytes[(randomness8 & 124) >> 2];
        utf8Destination[24] = Base32Bytes[((randomness8 & 3) << 3) | ((randomness9 & 224) >> 5)];

        bytesWritten = LengthInTextElements;
        return true;
    }

    // Comparable/Equatable

    public override int GetHashCode()
    {
        var span = AsSpan(in this);
        var ints = MemoryMarshal.Cast<byte, int>(span);

        // Simply XOR, same algorithm of Guid.GetHashCode
        return ints[0] ^ ints[1] ^ ints[2] ^ ints[3];
    }

    public bool Equals(Ulid other)
    {
        var thisSpan = AsSpan(in this);
        var otherSpan = AsSpan(in other);

        return thisSpan.SequenceEqual(otherSpan);
    }

    public static bool operator <(Ulid a, Ulid b) => a.CompareTo(b) < 0;

    public static bool operator <=(Ulid a, Ulid b) => a.CompareTo(b) <= 0;

    public static bool operator >(Ulid a, Ulid b) => a.CompareTo(b) > 0;

    public static bool operator >=(Ulid a, Ulid b) => a.CompareTo(b) >= 0;

    public int CompareTo(Ulid other)
    {
        var thisSpan = AsSpan(in this);
        var otherSpan = AsSpan(in other);
        return thisSpan.SequenceCompareTo(otherSpan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<byte> AsSpan(in Ulid ulid)
    {
        ref var refUlid = ref Unsafe.AsRef(in ulid);

        var span = MemoryMarshal.CreateReadOnlySpan(ref refUlid, 1);
        return MemoryMarshal.AsBytes(span);
    }

    public static explicit operator Guid(Ulid @this)
    {
        return @this.ToGuid();
    }

    /// <summary>
    /// Convert this <c>Ulid</c> value to a <c>Guid</c> value with the same comparability.
    /// </summary>
    /// <remarks>
    /// The byte arrangement between Ulid and Guid is not preserved.
    /// </remarks>
    /// <returns>The converted <c>Guid</c> value</returns>
    public Guid ToGuid()
    {
        Span<byte> buf = stackalloc byte[16];

        TryWriteBytes(buf);

        if (BitConverter.IsLittleEndian)
        {
            // Align with Guid layout - Int32, Int16, Int16, 8x Int8
            buf[0..4].Reverse();
            buf[4..6].Reverse();
            buf[6..8].Reverse();
        }
        return MemoryMarshal.Read<Guid>(buf);
    }
}
