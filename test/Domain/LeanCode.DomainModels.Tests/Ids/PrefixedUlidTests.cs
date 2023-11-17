using System.Globalization;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LeanCode.DomainModels.Ids;
using LeanCode.DomainModels.Ulids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.PrefixedUlid, CustomPrefix = "tpu")]
public readonly partial record struct TestPrefixedUlidId;

public class PrefixedUlidIdTests
{
    private const string TPUEmpty = "tpu_00000000000000000000000000";
    private const string TPU1 = "tpu_01H40FTH0DN6SDM42A77SBRMDB";
    private const string TPU2 = "tpu_01H40FTH0PWHEF7FYMY6XM7VE4";
    private const string TPU3 = "tpu_01H40PRZCA6XDXE41XTGQNG2ZH";
    private static readonly Ulid TPG1Ulid = Ulid.Parse("01H40FTH0DN6SDM42A77SBRMDB", CultureInfo.InvariantCulture);

    [Fact]
    public void Generated_class_implements_ITypedId()
    {
        new TestPrefixedUlidId().Should().BeAssignableTo(typeof(IPrefixedTypedId<TestPrefixedUlidId>));
    }

    [Fact]
    public void Default_and_empty_are_equal()
    {
        TestPrefixedUlidId.Empty.Should().Be(default);
        Assert.Equal(TestPrefixedUlidId.Empty, default);
    }

    [Fact]
    public void Default_value_has_empty_prefixed_ulid_value()
    {
        Assert.Equal(TPUEmpty, TestPrefixedUlidId.Empty.Value);
    }

    [Fact]
    public void From_null_behaves_correctly()
    {
        TestPrefixedUlidId.IsValid(null).Should().BeFalse();

        var parseNull = () => TestPrefixedUlidId.Parse(null!);
        var parseInvalid = () => TestPrefixedUlidId.ParseNullable("invalid");

        parseNull.Should().Throw<FormatException>();
        parseInvalid.Should().Throw<FormatException>();

        TestPrefixedUlidId.TryParse(null, out var value).Should().BeFalse();
        value.Should().Be(default);
    }

    [Fact]
    public void From_malformed_value_behaves_correctly()
    {
        TestPrefixedUlidId.IsValid("invalid").Should().BeFalse();

        var parse = () => TestPrefixedUlidId.Parse("invalid");
        var parseNullable = () => TestPrefixedUlidId.ParseNullable("invalid");

        parse.Should().Throw<FormatException>();
        parseNullable.Should().Throw<FormatException>();

        TestPrefixedUlidId.TryParse("invalid", out var value).Should().BeFalse();
        value.Should().Be(default);
    }

    [Fact]
    public void From_value_with_invalid_prefix_value_behaves_correctly()
    {
        TestPrefixedUlidId.IsValid("tpg2_01H40JX6P53DAZFCKTWQ5Y8XNV").Should().BeFalse();

        var parse = () => TestPrefixedUlidId.Parse("tpg2_01H40JXENAFPA2HM9ZGQPGY08D");
        var parseNullable = () => TestPrefixedUlidId.ParseNullable("tpg2_01H40JXTGXAVQQ8RVRH00E7FXM");

        parse.Should().Throw<FormatException>();
        parseNullable.Should().Throw<FormatException>();

        TestPrefixedUlidId.TryParse("tpg2_01H40JY78HME3GHCXCCEGKXRPK", out var value).Should().BeFalse();
        value.Should().Be(default);
    }

    [Fact]
    public void From_value_with_invalid_ulid_behaves_correctly()
    {
        TestPrefixedUlidId.IsValid("tpg_01H40JX6P53DAZFCKTWQ5Y8XNVADSFADS").Should().BeFalse();

        var parse = () => TestPrefixedUlidId.Parse("tpg_01H40JX6P53DAZFCKTWQ5Y8XNVADSFADS");
        var parseNullable = () => TestPrefixedUlidId.ParseNullable("tpg_01H40JX6P53DAZFCKTWQ5Y8XNVADSFADS");
        parse.Should().Throw<FormatException>();
        parseNullable.Should().Throw<FormatException>();

        TestPrefixedUlidId.TryParse("tpg_01H40JX6P53DAZFCKTWQ5Y8XNVADSFADS", out var value).Should().BeFalse();
        value.Should().Be(default);
    }

    [Fact]
    public void From_valid_value_behaves_correctly()
    {
        TestPrefixedUlidId.IsValid(TPU1).Should().BeTrue();

        new TestPrefixedUlidId(TPG1Ulid).Value.Should().Be(TPU1);
        TestPrefixedUlidId.Parse(TPU1).Value.Should().Be(TPU1);
        TestPrefixedUlidId.ParseNullable(TPU1).Value.Value.Should().Be(TPU1);

        TestPrefixedUlidId.TryParse(TPU1, out var value).Should().BeTrue();
        value.Value.Should().Be(TPU1);
    }

    [Fact]
    public void Equals_behaves_correctly()
    {
        TestPrefixedUlidId.Parse(TPU1).Should().Be(TestPrefixedUlidId.Parse(TPU1));

        TestPrefixedUlidId.Parse(TPU1).Should().NotBe(TestPrefixedUlidId.Parse(TPU2));
        TestPrefixedUlidId.Parse(TPU1).Should().NotBeNull();
    }

    [Fact]
    public void CompareTo_behaves_correctly()
    {
        TestPrefixedUlidId.Parse(TPU1).CompareTo(TestPrefixedUlidId.Parse(TPU1)).Should().Be(0);
        TestPrefixedUlidId.Parse(TPU1).CompareTo(TestPrefixedUlidId.Parse(TPU2)).Should().BeNegative();
        TestPrefixedUlidId.Parse(TPU2).CompareTo(TestPrefixedUlidId.Parse(TPU1)).Should().BePositive();
    }

    [Fact]
    public void Comparisons_behave_correctly()
    {
        (TestPrefixedUlidId.Parse(TPU1) < TestPrefixedUlidId.Parse(TPU2)).Should().BeTrue();
        (TestPrefixedUlidId.Parse(TPU2) < TestPrefixedUlidId.Parse(TPU2)).Should().BeFalse();
        (TestPrefixedUlidId.Parse(TPU3) < TestPrefixedUlidId.Parse(TPU2)).Should().BeFalse();

        (TestPrefixedUlidId.Parse(TPU1) <= TestPrefixedUlidId.Parse(TPU2)).Should().BeTrue();
        (TestPrefixedUlidId.Parse(TPU2) <= TestPrefixedUlidId.Parse(TPU2)).Should().BeTrue();
        (TestPrefixedUlidId.Parse(TPU3) <= TestPrefixedUlidId.Parse(TPU2)).Should().BeFalse();

        (TestPrefixedUlidId.Parse(TPU1) > TestPrefixedUlidId.Parse(TPU2)).Should().BeFalse();
        (TestPrefixedUlidId.Parse(TPU2) > TestPrefixedUlidId.Parse(TPU2)).Should().BeFalse();
        (TestPrefixedUlidId.Parse(TPU3) > TestPrefixedUlidId.Parse(TPU2)).Should().BeTrue();

        (TestPrefixedUlidId.Parse(TPU1) >= TestPrefixedUlidId.Parse(TPU2)).Should().BeFalse();
        (TestPrefixedUlidId.Parse(TPU2) >= TestPrefixedUlidId.Parse(TPU2)).Should().BeTrue();
        (TestPrefixedUlidId.Parse(TPU3) >= TestPrefixedUlidId.Parse(TPU2)).Should().BeTrue();

        (TestPrefixedUlidId.Parse(TPU1) == TestPrefixedUlidId.Parse(TPU1)).Should().BeTrue();
        (TestPrefixedUlidId.Parse(TPU1) == TestPrefixedUlidId.Parse(TPU2)).Should().BeFalse();
        (TestPrefixedUlidId.Parse(TPU1) != TestPrefixedUlidId.Parse(TPU2)).Should().BeTrue();
        (TestPrefixedUlidId.Parse(TPU1) != TestPrefixedUlidId.Parse(TPU1)).Should().BeFalse();
    }

    [Fact]
    public void The_hash_code_is_equal_to_underylying_value()
    {
        TPU1.GetHashCode(StringComparison.Ordinal).Should().Be(TestPrefixedUlidId.Parse(TPU1).GetHashCode());
    }

    [Fact]
    public void Casts_to_underlying_type_extract_the_value()
    {
        string implicitValue = TestPrefixedUlidId.Parse(TPU1);
        var explicitValue = (string)TestPrefixedUlidId.Parse(TPU2);

        implicitValue.Should().Be(TPU1);
        explicitValue.Should().Be(TPU2);
    }

    [Fact]
    public void ToString_returns_string_representation_of_the_underlying_value()
    {
        TestPrefixedUlidId.Parse(TPU1).ToString().Should().Be(TPU1);
    }

    [Fact]
    public void ToString_from_ulid_ctor_returns_the_ulid_prefixed()
    {
        new TestPrefixedUlidId(TPG1Ulid).ToString().Should().Be(TPU1);
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_to_from_JSON()
    {
        var value = TestPrefixedUlidId.Parse(TPU1);

        var json = JsonSerializer.Serialize(value);
        var deserialized = JsonSerializer.Deserialize<TestPrefixedUlidId>(json);

        deserialized.Should().Be(value);
    }

    [Fact]
    public void The_type_serializes_as_raw_underlying_type()
    {
        var value = TestPrefixedUlidId.Parse(TPU1);

        var json = JsonSerializer.Serialize(value);

        json.Should().Be("\"" + TPU1 + "\"");
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_as_dictionary_key_from_JSON()
    {
        var value = TestPrefixedUlidId.Parse(TPU1);
        var dict = new Dictionary<TestPrefixedUlidId, int> { [value] = 1 };

        var json = JsonSerializer.Serialize(dict);
        var deserialized = JsonSerializer.Deserialize<Dictionary<TestPrefixedUlidId, int>>(json);

        deserialized.Should().BeEquivalentTo(dict);
    }

    [Fact]
    public void New_generates_random_ID()
    {
        var rnd = TestPrefixedUlidId.New();

        rnd.Value.Should().StartWith("tpu_");

        Ulid.TryParse(rnd.Value[4..], out _).Should().BeTrue();
    }

    [Fact]
    public void Database_expressions_work()
    {
        DatabaseExpressionsWork<TestPrefixedUlidId>();

        static void DatabaseExpressionsWork<T>()
            where T : struct, IPrefixedTypedId<T>
        {
            T.FromDatabase.Compile().Invoke(TPU1).Should().Be(T.Parse(TPU1));
            T.DatabaseEquals.Compile().Invoke(T.Parse(TPU1), T.Parse(TPU1)).Should().BeTrue();
        }
    }

    [Fact]
    public void RawLength_is_correct()
    {
        TestPrefixedUlidId.RawLength.Should().Be(TPU1.Length);
    }

    [Fact]
    public void Raw_ulid_can_be_extracted_from_type()
    {
        var id = TestPrefixedUlidId.Parse(TPU1);
        var ulid = id.Ulid;

        ulid.Should().Be(TPG1Ulid);
    }

    [Fact]
    public void Ids_are_case_insensitive()
    {
        var u1 = TestPrefixedUlidId.Parse("tpu_01ARZ3NDEKTSV4RRFFQ69G5FAV");
        var u2 = TestPrefixedUlidId.Parse("tpu_01arz3ndektsv4rrffq69g5fav");

        u1.Should().Be(u2);
    }

    [Fact]
    public void TryFormatChar_is_correct()
    {
        var id = TestPrefixedUlidId.Parse(TPU1);
        var buffer = new char[50];

        id.TryFormat(buffer.AsSpan(0, 15), out var charsWritten, "", null).Should().BeFalse();
        charsWritten.Should().Be(0);

        id.TryFormat(buffer, out charsWritten, "", null).Should().BeTrue();
        charsWritten.Should().Be(TestPrefixedUlidId.RawLength);
        new string(buffer[..TestPrefixedUlidId.RawLength]).Should().Be(TPU1);
        buffer[TestPrefixedUlidId.RawLength..].Should().AllBeEquivalentTo(default(char));
    }

    [Fact]
    public void TryFormatUtf8Byte_is_correct()
    {
        var id = TestPrefixedUlidId.Parse(TPU1);
        var buffer = new byte[50];
        var expectedBytes = Encoding.UTF8.GetBytes(TPU1);

        id.TryFormat(buffer.AsSpan(0, 15), out var bytesWritten, "", null).Should().BeFalse();
        bytesWritten.Should().Be(0);

        id.TryFormat(buffer, out bytesWritten, "", null).Should().BeTrue();
        bytesWritten.Should().Be(TestPrefixedUlidId.RawLength);
        buffer[..TestPrefixedUlidId.RawLength].Should().BeEquivalentTo(expectedBytes);
        buffer[TestPrefixedUlidId.RawLength..].Should().AllBeEquivalentTo(default(byte));
    }
}
