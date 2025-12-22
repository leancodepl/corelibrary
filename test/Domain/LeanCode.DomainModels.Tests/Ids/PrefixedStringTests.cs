using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LeanCode.DomainModels.Ids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.PrefixedString, CustomPrefix = "tps", MaxValueLength = 10)]
public readonly partial record struct TestPrefixedStringId;

public class PrefixedStringIdTests
{
    private const string TPSEmpty = "";
    private const string TPS1 = "tps_abc123";
    private const string TPS2 = "tps_def456";
    private const string TPS3 = "tps_xyz789";

    [Fact]
    [SuppressMessage("?", "xUnit2007", Justification = "Cannot use `IPrefixedTypedId` as generic parameter.")]
    public void Generated_class_implements_ITypedId()
    {
        Assert.IsAssignableFrom(typeof(IPrefixedTypedId<TestPrefixedStringId>), new TestPrefixedStringId());
    }

    [Fact]
    public void Default_and_empty_are_equal()
    {
        Assert.Equal(TestPrefixedStringId.Empty, default);
    }

    [Fact]
    public void Default_value_has_empty_string_value()
    {
        Assert.Equal(TPSEmpty, TestPrefixedStringId.Empty.Value);
    }

    [Fact]
    public void From_null_behaves_correctly()
    {
        Assert.False(TestPrefixedStringId.IsValid(null));

        Assert.Throws<FormatException>(() => TestPrefixedStringId.Parse(null!));
        Assert.Throws<FormatException>(() => TestPrefixedStringId.ParseNullable("invalid"));
        Assert.False(TestPrefixedStringId.TryParse(null, out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_malformed_value_behaves_correctly()
    {
        Assert.False(TestPrefixedStringId.IsValid("invalid"));

        Assert.Throws<FormatException>(() => TestPrefixedStringId.Parse("invalid"));
        Assert.Throws<FormatException>(() => TestPrefixedStringId.ParseNullable("invalid"));
        Assert.False(TestPrefixedStringId.TryParse("invalid", out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_value_with_invalid_prefix_behaves_correctly()
    {
        Assert.False(TestPrefixedStringId.IsValid("tps2_abc123"));

        Assert.Throws<FormatException>(() => TestPrefixedStringId.Parse("tps2_abc123"));
        Assert.Throws<FormatException>(() => TestPrefixedStringId.ParseNullable("tps2_abc123"));
        Assert.False(TestPrefixedStringId.TryParse("tps2_abc123", out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_value_without_separator_behaves_correctly()
    {
        Assert.False(TestPrefixedStringId.IsValid("tpsabc123"));

        Assert.Throws<FormatException>(() => TestPrefixedStringId.Parse("tpsabc123"));
        Assert.False(TestPrefixedStringId.TryParse("tpsabc123", out _));
    }

    [Fact]
    public void Empty_value_part_behaves_correctly()
    {
        Assert.True(TestPrefixedStringId.IsValid("tps_"));

        var id = TestPrefixedStringId.Parse("tps_");
        Assert.Equal("tps_", id.Value);
        Assert.Equal(0, id.ValuePart.Length);

        Assert.True(TestPrefixedStringId.TryParse("tps_", out var parsed));
        Assert.Equal("tps_", parsed.Value);
        Assert.Equal(0, parsed.ValuePart.Length);
    }

    [Fact]
    public void From_valid_value_behaves_correctly()
    {
        Assert.True(TestPrefixedStringId.IsValid(TPS1));

        Assert.Equal(TPS1, TestPrefixedStringId.Parse(TPS1).Value);
        Assert.Equal(TPS1, TestPrefixedStringId.ParseNullable(TPS1)!.Value.Value);
        Assert.True(TestPrefixedStringId.TryParse(TPS1, out var value));
        Assert.Equal(TPS1, value.Value);
    }

    [Fact]
    public void FromValuePart_creates_prefixed_id()
    {
        var id = TestPrefixedStringId.FromValuePart("abc123");
        Assert.Equal(TPS1, id.Value);
    }

    [Fact]
    public void ValuePart_extracts_value_part()
    {
        var id = TestPrefixedStringId.Parse(TPS1);
        Assert.Equal("abc123", id.ValuePart);
    }

    [Fact]
    public void Destructure_extracts_prefix_and_data()
    {
        var id = TestPrefixedStringId.Parse(TPS1);
        var (prefix, data) = id.Destructure();

        Assert.Equal("tps", prefix);
        Assert.Equal("abc123", data);
    }

    [Fact]
    public void Equals_behaves_correctly()
    {
        Assert.True(TestPrefixedStringId.Parse(TPS1).Equals(TestPrefixedStringId.Parse(TPS1)));
        Assert.False(TestPrefixedStringId.Parse(TPS1).Equals(TestPrefixedStringId.Parse(TPS2)));
        Assert.False(TestPrefixedStringId.Parse(TPS1).Equals(null));
    }

    [Fact]
    public void CompareTo_behaves_correctly()
    {
        Assert.Equal(0, TestPrefixedStringId.Parse(TPS1).CompareTo(TestPrefixedStringId.Parse(TPS1)));
        Assert.True(TestPrefixedStringId.Parse(TPS1).CompareTo(TestPrefixedStringId.Parse(TPS2)) < 0);
        Assert.True(TestPrefixedStringId.Parse(TPS2).CompareTo(TestPrefixedStringId.Parse(TPS1)) > 0);
    }

    [Fact]
    public void Comparisons_behave_correctly()
    {
        Assert.True(TestPrefixedStringId.Parse(TPS1) < TestPrefixedStringId.Parse(TPS2));
        Assert.False(TestPrefixedStringId.Parse(TPS2) < TestPrefixedStringId.Parse(TPS2));
        Assert.False(TestPrefixedStringId.Parse(TPS3) < TestPrefixedStringId.Parse(TPS2));

        Assert.True(TestPrefixedStringId.Parse(TPS1) <= TestPrefixedStringId.Parse(TPS2));
        Assert.True(TestPrefixedStringId.Parse(TPS2) <= TestPrefixedStringId.Parse(TPS2));
        Assert.False(TestPrefixedStringId.Parse(TPS3) <= TestPrefixedStringId.Parse(TPS2));

        Assert.False(TestPrefixedStringId.Parse(TPS1) > TestPrefixedStringId.Parse(TPS2));
        Assert.False(TestPrefixedStringId.Parse(TPS2) > TestPrefixedStringId.Parse(TPS2));
        Assert.True(TestPrefixedStringId.Parse(TPS3) > TestPrefixedStringId.Parse(TPS2));

        Assert.False(TestPrefixedStringId.Parse(TPS1) >= TestPrefixedStringId.Parse(TPS2));
        Assert.True(TestPrefixedStringId.Parse(TPS2) >= TestPrefixedStringId.Parse(TPS2));
        Assert.True(TestPrefixedStringId.Parse(TPS3) >= TestPrefixedStringId.Parse(TPS2));

        Assert.True(TestPrefixedStringId.Parse(TPS1) == TestPrefixedStringId.Parse(TPS1));
        Assert.False(TestPrefixedStringId.Parse(TPS1) == TestPrefixedStringId.Parse(TPS2));
        Assert.True(TestPrefixedStringId.Parse(TPS1) != TestPrefixedStringId.Parse(TPS2));
        Assert.False(TestPrefixedStringId.Parse(TPS1) != TestPrefixedStringId.Parse(TPS1));
    }

    [Fact]
    public void The_hash_code_is_equal_to_underlying_value()
    {
        Assert.Equal(TPS1.GetHashCode(StringComparison.Ordinal), TestPrefixedStringId.Parse(TPS1).GetHashCode());
    }

    [Fact]
    public void Casts_to_underlying_type_extract_the_value()
    {
        string implicitValue = TestPrefixedStringId.Parse(TPS1);
        var explicitValue = (string)TestPrefixedStringId.Parse(TPS2);

        Assert.Equal(TPS1, implicitValue);
        Assert.Equal(TPS2, explicitValue);
    }

    [Fact]
    public void ToString_returns_string_representation_of_the_underlying_value()
    {
        Assert.Equal(TPS1, TestPrefixedStringId.Parse(TPS1).ToString());
    }

    [Fact]
    public void ToString_from_FromValuePart_returns_the_prefixed_value()
    {
        Assert.Equal(TPS1, TestPrefixedStringId.FromValuePart("abc123").ToString());
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_to_from_JSON()
    {
        var value = TestPrefixedStringId.Parse(TPS1);

        var json = JsonSerializer.Serialize(value);
        var deserialized = JsonSerializer.Deserialize<TestPrefixedStringId>(json);

        Assert.Equal(value, deserialized);
    }

    [Fact]
    public void The_type_serializes_as_raw_underlying_type()
    {
        var value = TestPrefixedStringId.Parse(TPS1);

        var json = JsonSerializer.Serialize(value);

        Assert.Equal("\"" + TPS1 + "\"", json);
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_as_dictionary_key_from_JSON()
    {
        var value = TestPrefixedStringId.Parse(TPS1);
        var dict = new Dictionary<TestPrefixedStringId, int> { [value] = 1 };

        var json = JsonSerializer.Serialize(dict);
        var deserialized = JsonSerializer.Deserialize<Dictionary<TestPrefixedStringId, int>>(json);

        Assert.Equal(dict, deserialized);
    }

    [Fact]
    public void Database_expressions_work()
    {
        DatabaseExpressionsWork<TestPrefixedStringId>();

        static void DatabaseExpressionsWork<T>()
            where T : struct, IPrefixedTypedId<T>
        {
            Assert.Equal(T.FromDatabase.Compile().Invoke(TPS1), T.Parse(TPS1));
            Assert.True(T.DatabaseEquals.Compile().Invoke(T.Parse(TPS1), T.Parse(TPS1)));
        }
    }

    [Fact]
    public void TryFormatChar_is_correct()
    {
        var id = TestPrefixedStringId.Parse(TPS1);
        var buffer = new char[50];

        id.TryFormat(buffer.AsSpan(0, 5), out var charsWritten, "", null).Should().BeFalse();
        charsWritten.Should().Be(0);

        id.TryFormat(buffer, out charsWritten, "", null).Should().BeTrue();
        charsWritten.Should().Be(TPS1.Length);
        new string(buffer[..TPS1.Length]).Should().Be(TPS1);
        buffer[TPS1.Length..].Should().AllBeEquivalentTo(default(char));
    }

    [Fact]
    public void TryFormatUtf8Byte_is_correct()
    {
        var id = TestPrefixedStringId.Parse(TPS1);
        var buffer = new byte[50];
        var expectedBytes = Encoding.UTF8.GetBytes(TPS1);

        id.TryFormat(buffer.AsSpan(0, 5), out var bytesWritten, "", null).Should().BeFalse();
        bytesWritten.Should().Be(0);

        id.TryFormat(buffer, out bytesWritten, "", null).Should().BeTrue();
        bytesWritten.Should().Be(TPS1.Length);
        buffer[..TPS1.Length].Should().BeEquivalentTo(expectedBytes);
        buffer[TPS1.Length..].Should().AllBeEquivalentTo(default(byte));
    }

    [Fact]
    public void IsEmpty_works_correctly()
    {
        Assert.True(TestPrefixedStringId.Empty.IsEmpty);
        Assert.False(TestPrefixedStringId.Parse(TPS1).IsEmpty);
    }

    [Fact]
    public void MaxValueLength_is_exposed()
    {
        Assert.Equal(10, TestPrefixedStringId.MaxValueLength);
    }

    [Fact]
    public void MaxLength_is_calculated_correctly()
    {
        // prefix "tpm" (3) + separator "_" (1) + max value part (10) = 14
        Assert.Equal(14, TestPrefixedStringId.MaxLength);
    }

    [Fact]
    public void String_within_max_length_is_valid()
    {
        Assert.True(TestPrefixedStringId.IsValid("tps_1234567890")); // value part = 10 chars
        Assert.True(TestPrefixedStringId.IsValid("tps_short"));
    }

    [Fact]
    public void String_exceeding_max_length_is_invalid()
    {
        Assert.False(TestPrefixedStringId.IsValid("tps_12345678901")); // value part = 11 chars

        Assert.Throws<FormatException>(() => TestPrefixedStringId.Parse("tps_12345678901"));
        Assert.False(TestPrefixedStringId.TryParse("tps_this_is_too_long", out _));
    }

    [Fact]
    public void FromValuePart_validates_max_length()
    {
        // Within limit - should work
        var id = TestPrefixedStringId.FromValuePart("1234567890");
        Assert.Equal("tps_1234567890", id.Value);

        // Exceeds limit - should throw
        Assert.Throws<ArgumentException>(() => TestPrefixedStringId.FromValuePart("12345678901"));
    }
}
