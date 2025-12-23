using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LeanCode.DomainModels.Ids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.RawString, MaxValueLength = 10)]
public readonly partial record struct TestRawStringId;

public class RawStringIdTests
{
    private const string String1 = "abc123";
    private const string String2 = "def456";
    private const string String3 = "xyz789";

    [Fact]
    [SuppressMessage("?", "xUnit2007", Justification = "Cannot use `IRawStringTypedId` as generic parameter.")]
    public void Generated_class_implements_ITypedId()
    {
        Assert.IsAssignableFrom(typeof(IRawStringTypedId<TestRawStringId>), new TestRawStringId());
    }

    [Fact]
    public void Default_and_empty_are_equal()
    {
        Assert.Equal(TestRawStringId.Empty, default);
    }

    [Fact]
    public void Default_value_has_empty_string_value()
    {
        Assert.Equal(string.Empty, TestRawStringId.Empty.Value);
    }

    [Fact]
    public void Creating_value_out_of_string_works()
    {
        Assert.Equal(String1, new TestRawStringId(String1).Value);
    }

    [Fact]
    public void From_null_behaves_correctly()
    {
        Assert.False(TestRawStringId.IsValid(null));

        Assert.Throws<FormatException>(() => TestRawStringId.Parse(null!));
        Assert.Null(TestRawStringId.ParseNullable(null));
        Assert.False(TestRawStringId.TryParse(null, out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_string_behaves_correctly()
    {
        Assert.True(TestRawStringId.IsValid(String1));

        Assert.Equal(new TestRawStringId(String1), TestRawStringId.Parse(String1));
        Assert.Equal(new TestRawStringId(String1), TestRawStringId.ParseNullable(String1));
        Assert.True(TestRawStringId.TryParse(String1, out var value));
        Assert.Equal(new TestRawStringId(String1), value);
    }

    [Fact]
    public void Empty_string_is_valid()
    {
        Assert.True(TestRawStringId.IsValid(string.Empty));
        Assert.Equal(string.Empty, TestRawStringId.Parse(string.Empty).Value);
    }

    [Fact]
    public void Equals_behaves_correctly()
    {
        Assert.True(new TestRawStringId(String1).Equals(new TestRawStringId(String1)));
        Assert.False(new TestRawStringId(String1).Equals(new TestRawStringId(String2)));
        Assert.False(new TestRawStringId(String1).Equals(null));
    }

    [Fact]
    public void CompareTo_behaves_correctly()
    {
        Assert.Equal(0, new TestRawStringId(String1).CompareTo(new TestRawStringId(String1)));
        Assert.True(new TestRawStringId(String1).CompareTo(new TestRawStringId(String2)) < 0);
        Assert.True(new TestRawStringId(String2).CompareTo(new TestRawStringId(String1)) > 0);
    }

    [Fact]
    public void Comparisons_behave_correctly()
    {
        Assert.True(new TestRawStringId(String1) < new TestRawStringId(String2));
        Assert.False(new TestRawStringId(String2) < new TestRawStringId(String2));
        Assert.False(new TestRawStringId(String3) < new TestRawStringId(String2));

        Assert.True(new TestRawStringId(String1) <= new TestRawStringId(String2));
        Assert.True(new TestRawStringId(String2) <= new TestRawStringId(String2));
        Assert.False(new TestRawStringId(String3) <= new TestRawStringId(String2));

        Assert.False(new TestRawStringId(String1) > new TestRawStringId(String2));
        Assert.False(new TestRawStringId(String2) > new TestRawStringId(String2));
        Assert.True(new TestRawStringId(String3) > new TestRawStringId(String2));

        Assert.False(new TestRawStringId(String1) >= new TestRawStringId(String2));
        Assert.True(new TestRawStringId(String2) >= new TestRawStringId(String2));
        Assert.True(new TestRawStringId(String3) >= new TestRawStringId(String2));

        Assert.True(new TestRawStringId(String1) == new TestRawStringId(String1));
        Assert.False(new TestRawStringId(String1) == new TestRawStringId(String2));
        Assert.True(new TestRawStringId(String1) != new TestRawStringId(String2));
        Assert.False(new TestRawStringId(String1) != new TestRawStringId(String1));
    }

    [Fact]
    public void The_hash_code_is_equal_to_underlying_value()
    {
        Assert.Equal(String1.GetHashCode(StringComparison.Ordinal), new TestRawStringId(String1).GetHashCode());
    }

    [Fact]
    public void Casts_to_underlying_type_extract_the_value()
    {
        string implicitValue = new TestRawStringId(String1);
        var explicitValue = (string)new TestRawStringId(String2);

        Assert.Equal(String1, implicitValue);
        Assert.Equal(String2, explicitValue);
    }

    [Fact]
    public void ToString_returns_string_representation_of_the_underlying_value()
    {
        Assert.Equal(String1, new TestRawStringId(String1).ToString());
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_to_from_JSON()
    {
        var value = new TestRawStringId(String1);

        var json = JsonSerializer.Serialize(value);
        var deserialized = JsonSerializer.Deserialize<TestRawStringId>(json);

        Assert.Equal(value, deserialized);
    }

    [Fact]
    public void The_type_serializes_as_raw_underlying_type()
    {
        var value = new TestRawStringId(String1);

        var json = JsonSerializer.Serialize(value);

        Assert.Equal("\"" + String1 + "\"", json);
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_as_dictionary_key_from_JSON()
    {
        var value = new TestRawStringId(String1);
        var dict = new Dictionary<TestRawStringId, int> { [value] = 1 };

        var json = JsonSerializer.Serialize(dict);
        var deserialized = JsonSerializer.Deserialize<Dictionary<TestRawStringId, int>>(json);

        Assert.Equal(dict, deserialized);
    }

    [Fact]
    public void Database_expressions_work()
    {
        DatabaseExpressionsWork<TestRawStringId>();

        static void DatabaseExpressionsWork<T>()
            where T : struct, IRawStringTypedId<T>
        {
            var str = "test_value";
            Assert.Equal(T.FromDatabase.Compile().Invoke(str), T.Parse(str));
            Assert.True(T.DatabaseEquals.Compile().Invoke(T.Parse(str), T.Parse(str)));
        }
    }

    [Fact]
    public void TryFormatChar_is_correct()
    {
        var id = TestRawStringId.Parse(String1);
        var buffer = new char[50];

        id.TryFormat(buffer.AsSpan(0, 3), out var charsWritten, "", null).Should().BeFalse();
        charsWritten.Should().Be(0);

        id.TryFormat(buffer, out charsWritten, "", null).Should().BeTrue();
        charsWritten.Should().Be(String1.Length);
        new string(buffer[..String1.Length]).Should().Be(String1);
        buffer[String1.Length..].Should().AllBeEquivalentTo(default(char));
    }

    [Fact]
    public void TryFormatUtf8Byte_is_correct()
    {
        var id = TestRawStringId.Parse(String1);
        var buffer = new byte[50];
        var expectedBytes = Encoding.UTF8.GetBytes(String1);

        id.TryFormat(buffer.AsSpan(0, 3), out var bytesWritten, "", null).Should().BeFalse();
        bytesWritten.Should().Be(0);

        id.TryFormat(buffer, out bytesWritten, "", null).Should().BeTrue();
        bytesWritten.Should().Be(String1.Length);
        buffer[..String1.Length].Should().BeEquivalentTo(expectedBytes);
        buffer[String1.Length..].Should().AllBeEquivalentTo(default(byte));
    }

    [Fact]
    public void IsEmpty_works_correctly()
    {
        Assert.True(TestRawStringId.Empty.IsEmpty);
        Assert.True(new TestRawStringId(string.Empty).IsEmpty);
        Assert.False(new TestRawStringId(String1).IsEmpty);
    }

    [Fact]
    public void MaxLength_is_exposed()
    {
        Assert.Equal(10, TestRawStringId.MaxLength);
    }

    [Fact]
    public void String_within_max_length_is_valid()
    {
        Assert.True(TestRawStringId.IsValid("1234567890"));
        Assert.True(TestRawStringId.IsValid("short"));
    }

    [Fact]
    public void String_exceeding_max_length_is_invalid()
    {
        Assert.False(TestRawStringId.IsValid("12345678901")); // 11 chars

        Assert.Throws<FormatException>(() => TestRawStringId.Parse("12345678901"));
        Assert.False(TestRawStringId.TryParse("this_is_too_long", out _));
    }

    [Fact]
    public void Empty_string_is_valid_with_max_length()
    {
        Assert.True(TestRawStringId.IsValid(string.Empty));
    }
}
