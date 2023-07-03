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

using System.Globalization;
using FluentAssertions;
using LeanCode.DomainModels.Ulids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ulids;

public class UlidTest
{
    [Fact]
    public void New_ByteEquals_ToString_Equals()
    {
        for (int i = 0; i < 100; i++)
        {
            {
                var ulid = Ulid.NewUlid();
                var nulid = new NUlid.Ulid(ulid.ToByteArray());

                ulid.ToByteArray().Should().BeEquivalentTo(nulid.ToByteArray());
                ulid.ToString().Should().Be(nulid.ToString());
                ulid.Equals(ulid).Should().BeTrue();
                ulid.Equals(Ulid.NewUlid()).Should().BeFalse();
            }
            {
                var nulid = NUlid.Ulid.NewUlid();
                var ulid = new Ulid(nulid.ToByteArray());

                ulid.ToByteArray().Should().BeEquivalentTo(nulid.ToByteArray());
                ulid.ToString().Should().Be(nulid.ToString());
                ulid.Equals(ulid).Should().BeTrue();
                ulid.Equals(Ulid.NewUlid()).Should().BeFalse();
            }
        }
    }

    [Fact]
    public void Compare_Time()
    {
        var times = new DateTimeOffset[]
        {
            new DateTime(2012, 12, 4),
            new DateTime(2011, 12, 31),
            new DateTime(2012, 1, 5),
            new DateTime(2013, 12, 4),
            new DateTime(2016, 12, 4),
        };

        times
            .Select(x => Ulid.NewUlid(x))
            .OrderBy(x => x)
            .Select(x => x.Time)
            .Should()
            .BeEquivalentTo(times.OrderBy(x => x));
    }

    [Fact]
    public void Parse()
    {
        for (var i = 0; i < 100; i++)
        {
            var nulid = NUlid.Ulid.NewUlid();
            Ulid.Parse(nulid.ToString(), CultureInfo.InvariantCulture)
                .ToByteArray()
                .Should()
                .BeEquivalentTo(nulid.ToByteArray());
        }
    }

    [Fact]
    public void Randomness()
    {
        var d = DateTime.Parse("1970/1/1 00:00:00Z", CultureInfo.InvariantCulture);
        var r = new byte[10];
        var first = Ulid.NewUlid(d, r);
        var second = Ulid.NewUlid(d, r);
        first.ToString().Should().BeEquivalentTo(second.ToString());
        // Console.WriteLine($"first={first.ToString()}, second={second.ToString()}");
    }

    [Fact]
    public void GuidInterop()
    {
        var ulid = Ulid.NewUlid();
        var guid = ulid.ToGuid();
        var ulid2 = new Ulid(guid);

        ulid2.Should().BeEquivalentTo(ulid, "a Ulid-Guid roundtrip should result in identical values");
    }

    [Fact]
    public void GuidComparison()
    {
        var dataSmaller = new byte[] { 0, 255, 255, 255, 255, 255, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var dataLarger = new byte[] { 1, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var ulidSmaller = new Ulid(dataSmaller);
        var ulidLarger = new Ulid(dataLarger);

        var guidSmaller = ulidSmaller.ToGuid();
        var guidLarger = ulidLarger.ToGuid();

        ulidSmaller.CompareTo(ulidLarger).Should().BeLessThan(0, "a Ulid comparison should compare byte to byte");
        guidSmaller.CompareTo(guidLarger).Should().BeLessThan(0, "a Ulid to Guid cast should preserve order");
    }

    [Fact]
    public void UlidParseRejectsInvalidStrings()
    {
        Assert.Throws<ArgumentException>(() => Ulid.Parse("1234", CultureInfo.InvariantCulture));
        Assert.Throws<ArgumentException>(() => Ulid.Parse(Guid.NewGuid().ToString(), CultureInfo.InvariantCulture));
    }

    [Fact]
    public void UlidTryParseFailsForInvalidStrings()
    {
        Assert.False(Ulid.TryParse("1234", out _));
        Assert.False(Ulid.TryParse(Guid.NewGuid().ToString(), out _));
    }

    [Fact]
    public void Ulids_are_case_insenstive()
    {
        var u1 = Ulid.Parse("01ARZ3NDEKTSV4RRFFQ69G5FAV", CultureInfo.InvariantCulture);
        var u2 = Ulid.Parse("01arz3ndektsv4rrffq69g5fav", CultureInfo.InvariantCulture);

        u1.Should().Be(u2);
    }

    [Fact]
    public void Generated_Ulids_are_close_to_now()
    {
        var now = DateTimeOffset.UtcNow;
        var ulid = Ulid.NewUlid();

        ulid.Time.Should().BeCloseTo(now, TimeSpan.FromMilliseconds(25));
    }
}
