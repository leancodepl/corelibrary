using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FluentAssertions;
using LeanCode.DomainModels.Ids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.RawInt)]
public readonly partial record struct TestIntId;

public class IntIdTests
{
    [Fact]
    [SuppressMessage("?", "xUnit2007", Justification = "Cannot use `IRawTypedId` as generic parameter.")]
    public void Generated_class_implements_ITypedId()
    {
        Assert.IsAssignableFrom(typeof(IRawTypedId<int, TestIntId>), new TestIntId());
    }

    [Fact]
    public void Default_and_empty_are_equal()
    {
        Assert.Equal(TestIntId.Empty, default);
    }

    [Fact]
    public void Default_value_has_0_value()
    {
        Assert.Equal(0, TestIntId.Empty.Value);
    }

    [Fact]
    public void From_null_behaves_correctly()
    {
        Assert.False(TestIntId.IsValid(null));

        Assert.Null(TestIntId.ParseNullable(null));
        Assert.False(TestIntId.TryParse(null, out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_number_behaves_correctly()
    {
        Assert.True(TestIntId.IsValid(789));

        Assert.Equal(123, new TestIntId(123).Value);
        Assert.Equal(new TestIntId(321), TestIntId.Parse(321));
        Assert.Equal(new TestIntId(321), TestIntId.ParseNullable(321));
        Assert.True(TestIntId.TryParse(456, out var value));
        Assert.Equal(new TestIntId(456), value);
    }

    [Fact]
    public void Equals_behaves_correctly()
    {
        Assert.True(new TestIntId(123).Equals(new TestIntId(123)));
        Assert.False(new TestIntId(321).Equals(new TestIntId(123)));
        Assert.False(new TestIntId(321).Equals(null));
    }

    [Fact]
    public void CompareTo_behaves_correctly()
    {
        Assert.Equal(0, new TestIntId(789).CompareTo(new TestIntId(789)));
        Assert.Equal(-1, new TestIntId(788).CompareTo(new TestIntId(789)));
        Assert.Equal(1, new TestIntId(788).CompareTo(new TestIntId(787)));
    }

    [Fact]
    public void Comparisons_behave_correctly()
    {
        Assert.True(new TestIntId(1) < new TestIntId(2));
        Assert.False(new TestIntId(2) < new TestIntId(2));
        Assert.False(new TestIntId(3) < new TestIntId(2));

        Assert.True(new TestIntId(1) <= new TestIntId(2));
        Assert.True(new TestIntId(2) <= new TestIntId(2));
        Assert.False(new TestIntId(3) <= new TestIntId(2));

        Assert.False(new TestIntId(1) > new TestIntId(2));
        Assert.False(new TestIntId(2) > new TestIntId(2));
        Assert.True(new TestIntId(3) > new TestIntId(2));

        Assert.False(new TestIntId(1) >= new TestIntId(2));
        Assert.True(new TestIntId(2) >= new TestIntId(2));
        Assert.True(new TestIntId(3) >= new TestIntId(2));

        Assert.True(new TestIntId(2) == new TestIntId(2));
        Assert.False(new TestIntId(3) == new TestIntId(2));
        Assert.True(new TestIntId(3) != new TestIntId(2));
        Assert.False(new TestIntId(3) != new TestIntId(3));
    }

    [Fact]
    public void The_hash_code_is_equal_to_the_underylying_value_hashcode()
    {
        Assert.Equal(123.GetHashCode(), new TestIntId(123).GetHashCode());
    }

    [Fact]
    public void Casts_to_underlying_type_extract_the_value()
    {
        int implicitValue = new TestIntId(123);
        var explicitValue = (int)new TestIntId(321);

        Assert.Equal(123, implicitValue);
        Assert.Equal(321, explicitValue);
    }

    [Fact]
    public void ToString_returns_string_representation_of_the_underlying_value()
    {
        Assert.Equal("345", new TestIntId(345).ToString());
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_to_from_JSON()
    {
        var value = new TestIntId(1234);

        var json = JsonSerializer.Serialize(value);
        var deserialized = JsonSerializer.Deserialize<TestIntId>(json);

        Assert.Equal(value, deserialized);
    }

    [Fact]
    public void The_type_serializes_as_raw_underlying_type()
    {
        var value = new TestIntId(1234);

        var json = JsonSerializer.Serialize(value);

        Assert.Equal("1234", json);
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_as_dictionary_key_from_JSON()
    {
        var value = new TestIntId(1234);
        var dict = new Dictionary<TestIntId, int> { [value] = 1 };

        var json = JsonSerializer.Serialize(dict);
        var deserialized = JsonSerializer.Deserialize<Dictionary<TestIntId, int>>(json);

        Assert.Equal(dict, deserialized);
    }

    [Fact]
    public void Database_expressions_work()
    {
        DatabaseExpressionsWork<TestIntId>();

        static void DatabaseExpressionsWork<T>()
            where T : struct, IRawTypedId<int, T>
        {
            Assert.Equal(T.FromDatabase.Compile().Invoke(1), T.Parse(1));
            Assert.True(T.DatabaseEquals.Compile().Invoke(T.Parse(1), T.Parse(1)));
        }
    }

    [Fact]
    public void TryFormatChar_is_correct()
    {
        var id = new TestIntId(1234);
        var buffer = new char[10];

        id.TryFormat(buffer.AsSpan(0, 2), out var charsWritten, "", null).Should().BeFalse();
        charsWritten.Should().Be(0);

        id.TryFormat(buffer, out charsWritten, "", null).Should().BeTrue();
        charsWritten.Should().Be(4);
        new string(buffer[..4]).Should().Be("1234");
        buffer[4..].Should().AllBeEquivalentTo(default(char));
    }

    [Fact]
    public void TryFormatUtf8Byte_is_correct()
    {
        var id = new TestIntId(1234);
        var buffer = new byte[10];
        var expectedBuffer = "1234"u8.ToArray();

        id.TryFormat(buffer.AsSpan(0, 2), out var bytesWritten, "", null).Should().BeFalse();
        bytesWritten.Should().Be(0);

        id.TryFormat(buffer, out bytesWritten, "", null).Should().BeTrue();
        bytesWritten.Should().Be(4);
        buffer[..4].Should().BeEquivalentTo(expectedBuffer);
        buffer[4..].Should().AllBeEquivalentTo(default(byte));
    }
}
