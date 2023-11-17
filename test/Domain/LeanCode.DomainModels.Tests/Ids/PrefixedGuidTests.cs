using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LeanCode.DomainModels.Ids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "tpg")]
public readonly partial record struct TestPrefixedGuidId;

public class PrefixedGuidIdTests
{
    private const string TPGEmpty = "tpg_00000000000000000000000000000000";
    private const string TPG1 = "tpg_0ba5ef95394b4d089cb1f47f3ebe41b3";
    private const string TPG2 = "tpg_50fee6a4af774befa52c6eb824032690";
    private const string TPG3 = "tpg_ed5b37141a2f4469a709ecf37fe791ca";
    private static readonly Guid TPG1Guid = Guid.ParseExact("0ba5ef95394b4d089cb1f47f3ebe41b3", "N");

    [Fact]
    [SuppressMessage("?", "xUnit2007", Justification = "Cannot use `IPrefixedTypedId` as generic parameter.")]
    public void Generated_class_implements_ITypedId()
    {
        Assert.IsAssignableFrom(typeof(IPrefixedTypedId<TestPrefixedGuidId>), new TestPrefixedGuidId());
    }

    [Fact]
    public void Default_and_empty_are_equal()
    {
        Assert.Equal(TestPrefixedGuidId.Empty, default);
    }

    [Fact]
    public void Default_value_has_empty_prefixed_guid_value()
    {
        Assert.Equal(TPGEmpty, TestPrefixedGuidId.Empty.Value);
    }

    [Fact]
    public void From_null_behaves_correctly()
    {
        Assert.False(TestPrefixedGuidId.IsValid(null));

        Assert.Throws<FormatException>(() => TestPrefixedGuidId.Parse(null!));
        Assert.Throws<FormatException>(() => TestPrefixedGuidId.ParseNullable("invalid"));
        Assert.False(TestPrefixedGuidId.TryParse(null, out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_malformed_value_behaves_correctly()
    {
        Assert.False(TestPrefixedGuidId.IsValid("invalid"));

        Assert.Throws<FormatException>(() => TestPrefixedGuidId.Parse("invalid"));
        Assert.Throws<FormatException>(() => TestPrefixedGuidId.ParseNullable("invalid"));
        Assert.False(TestPrefixedGuidId.TryParse("invalid", out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_value_with_invalid_prefix_value_behaves_correctly()
    {
        Assert.False(TestPrefixedGuidId.IsValid("tpg2_0ba5ef95394b4d089cb1f47f3ebe41b3"));

        Assert.Throws<FormatException>(() => TestPrefixedGuidId.Parse("tpg2_0ba5ef95394b4d089cb1f47f3ebe41b3"));
        Assert.Throws<FormatException>(() => TestPrefixedGuidId.ParseNullable("tpg2_0ba5ef95394b4d089cb1f47f3ebe41b3"));
        Assert.False(TestPrefixedGuidId.TryParse("tpg2_0ba5ef95394b4d089cb1f47f3ebe41b3", out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_value_with_invalid_guid_behaves_correctly()
    {
        Assert.False(TestPrefixedGuidId.IsValid("tpg_0b94b4d089cb1f47f3ebe41b3"));

        Assert.Throws<FormatException>(() => TestPrefixedGuidId.Parse("tpg_0b94b4d089cb1f47f3ebe41b3"));
        Assert.Throws<FormatException>(() => TestPrefixedGuidId.ParseNullable("tpg_0b94b4d089cb1f47f3ebe41b3"));
        Assert.False(TestPrefixedGuidId.TryParse("tpg_0b94b4d089cb1f47f3ebe41b3", out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_valid_value_behaves_correctly()
    {
        Assert.True(TestPrefixedGuidId.IsValid(TPG1));

        Assert.Equal(TPG1, new TestPrefixedGuidId(TPG1Guid).Value);
        Assert.Equal(TPG1, TestPrefixedGuidId.Parse(TPG1).Value);
        Assert.Equal(TPG1, TestPrefixedGuidId.ParseNullable(TPG1).Value);
        Assert.True(TestPrefixedGuidId.TryParse(TPG1, out var value));
        Assert.Equal(TPG1, value.Value);
    }

    [Fact]
    public void Equals_behaves_correctly()
    {
        Assert.True(TestPrefixedGuidId.Parse(TPG1).Equals(TestPrefixedGuidId.Parse(TPG1)));
        Assert.False(TestPrefixedGuidId.Parse(TPG1).Equals(TestPrefixedGuidId.Parse(TPG2)));
        Assert.False(TestPrefixedGuidId.Parse(TPG1).Equals(null));
    }

    [Fact]
    public void CompareTo_behaves_correctly()
    {
        Assert.Equal(0, TestPrefixedGuidId.Parse(TPG1).CompareTo(TestPrefixedGuidId.Parse(TPG1)));
        Assert.True(TestPrefixedGuidId.Parse(TPG1).CompareTo(TestPrefixedGuidId.Parse(TPG2)) <= -1);
        Assert.True(TestPrefixedGuidId.Parse(TPG2).CompareTo(TestPrefixedGuidId.Parse(TPG1)) >= 1);
    }

    [Fact]
    public void Comparisons_behave_correctly()
    {
        Assert.True(TestPrefixedGuidId.Parse(TPG1) < TestPrefixedGuidId.Parse(TPG2));
        Assert.False(TestPrefixedGuidId.Parse(TPG2) < TestPrefixedGuidId.Parse(TPG2));
        Assert.False(TestPrefixedGuidId.Parse(TPG3) < TestPrefixedGuidId.Parse(TPG2));

        Assert.True(TestPrefixedGuidId.Parse(TPG1) <= TestPrefixedGuidId.Parse(TPG2));
        Assert.True(TestPrefixedGuidId.Parse(TPG2) <= TestPrefixedGuidId.Parse(TPG2));
        Assert.False(TestPrefixedGuidId.Parse(TPG3) <= TestPrefixedGuidId.Parse(TPG2));

        Assert.False(TestPrefixedGuidId.Parse(TPG1) > TestPrefixedGuidId.Parse(TPG2));
        Assert.False(TestPrefixedGuidId.Parse(TPG2) > TestPrefixedGuidId.Parse(TPG2));
        Assert.True(TestPrefixedGuidId.Parse(TPG3) > TestPrefixedGuidId.Parse(TPG2));

        Assert.False(TestPrefixedGuidId.Parse(TPG1) >= TestPrefixedGuidId.Parse(TPG2));
        Assert.True(TestPrefixedGuidId.Parse(TPG2) >= TestPrefixedGuidId.Parse(TPG2));
        Assert.True(TestPrefixedGuidId.Parse(TPG3) >= TestPrefixedGuidId.Parse(TPG2));

        Assert.True(TestPrefixedGuidId.Parse(TPG1) == TestPrefixedGuidId.Parse(TPG1));
        Assert.False(TestPrefixedGuidId.Parse(TPG1) == TestPrefixedGuidId.Parse(TPG2));
        Assert.True(TestPrefixedGuidId.Parse(TPG1) != TestPrefixedGuidId.Parse(TPG2));
        Assert.False(TestPrefixedGuidId.Parse(TPG1) != TestPrefixedGuidId.Parse(TPG1));
    }

    [Fact]
    public void The_hash_code_is_equal_to_underylying_value()
    {
        Assert.Equal(TPG1.GetHashCode(StringComparison.Ordinal), TestPrefixedGuidId.Parse(TPG1).GetHashCode());
    }

    [Fact]
    public void Casts_to_underlying_type_extract_the_value()
    {
        string implicitValue = TestPrefixedGuidId.Parse(TPG1);
        var explicitValue = (string)TestPrefixedGuidId.Parse(TPG2);

        Assert.Equal(TPG1, implicitValue);
        Assert.Equal(TPG2, explicitValue);
    }

    [Fact]
    public void ToString_returns_string_representation_of_the_underlying_value()
    {
        Assert.Equal(TPG1, TestPrefixedGuidId.Parse(TPG1).ToString());
    }

    [Fact]
    public void ToString_from_guid_ctor_returns_the_guid_prefixed()
    {
        Assert.Equal(TPG1, new TestPrefixedGuidId(TPG1Guid).ToString());
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_to_from_JSON()
    {
        var value = TestPrefixedGuidId.Parse(TPG1);

        var json = JsonSerializer.Serialize(value);
        var deserialized = JsonSerializer.Deserialize<TestPrefixedGuidId>(json);

        Assert.Equal(value, deserialized);
    }

    [Fact]
    public void The_type_serializes_as_raw_underlying_type()
    {
        var value = TestPrefixedGuidId.Parse(TPG1);

        var json = JsonSerializer.Serialize(value);

        Assert.Equal("\"" + TPG1 + "\"", json);
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_as_dictionary_key_from_JSON()
    {
        var value = TestPrefixedGuidId.Parse(TPG1);
        var dict = new Dictionary<TestPrefixedGuidId, int> { [value] = 1 };

        var json = JsonSerializer.Serialize(dict);
        var deserialized = JsonSerializer.Deserialize<Dictionary<TestPrefixedGuidId, int>>(json);

        Assert.Equal(dict, deserialized);
    }

    [Fact]
    public void New_generates_random_ID()
    {
        var rnd = TestPrefixedGuidId.New();

        Assert.StartsWith("tpg_", rnd.Value, StringComparison.OrdinalIgnoreCase);
        Assert.True(Guid.TryParse(rnd.Value[4..], out _));
    }

    [Fact]
    public void Database_expressions_work()
    {
        DatabaseExpressionsWork<TestPrefixedGuidId>();

        static void DatabaseExpressionsWork<T>()
            where T : struct, IPrefixedTypedId<T>
        {
            Assert.Equal(T.FromDatabase.Compile().Invoke(TPG1), T.Parse(TPG1));
            Assert.True(T.DatabaseEquals.Compile().Invoke(T.Parse(TPG1), T.Parse(TPG1)));
        }
    }

    [Fact]
    public void RawLength_is_correct()
    {
        Assert.Equal(TestPrefixedGuidId.RawLength, TPG1.Length);
    }

    [Fact]
    public void TryFormatChar_is_correct()
    {
        var id = TestPrefixedGuidId.Parse(TPG1);
        var buffer = new char[50];

        id.TryFormat(buffer.AsSpan(0, 15), out var charsWritten, "", null).Should().BeFalse();
        charsWritten.Should().Be(0);

        id.TryFormat(buffer, out charsWritten, "", null).Should().BeTrue();
        charsWritten.Should().Be(TestPrefixedGuidId.RawLength);
        new string(buffer[..TestPrefixedGuidId.RawLength]).Should().Be(TPG1);
        buffer[TestPrefixedGuidId.RawLength..].Should().AllBeEquivalentTo(default(char));
    }

    [Fact]
    public void TryFormatUtf8Byte_is_correct()
    {
        var id = TestPrefixedGuidId.Parse(TPG1);
        var buffer = new byte[50];
        var expectedBytes = Encoding.UTF8.GetBytes(TPG1);

        id.TryFormat(buffer.AsSpan(0, 15), out var bytesWritten, "", null).Should().BeFalse();
        bytesWritten.Should().Be(0);

        id.TryFormat(buffer, out bytesWritten, "", null).Should().BeTrue();
        bytesWritten.Should().Be(TestPrefixedGuidId.RawLength);
        buffer[..TestPrefixedGuidId.RawLength].Should().BeEquivalentTo(expectedBytes);
        buffer[TestPrefixedGuidId.RawLength..].Should().AllBeEquivalentTo(default(byte));
    }
}
