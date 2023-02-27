using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using LeanCode.DomainModels.Ids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.RawGuid)]
public readonly partial record struct TestGuidId;

public class GuidIdTests
{
    private static readonly Guid Guid1 = Guid.Parse("0ba5ef95-394b-4d08-9cb1-f47f3ebe41b3");
    private static readonly Guid Guid2 = Guid.Parse("50fee6a4-af77-4bef-a52c-6eb824032690");
    private static readonly Guid Guid3 = Guid.Parse("ed5b3714-1a2f-4469-a709-ecf37fe791ca");

    [Fact]
    [SuppressMessage("?", "xUnit2007", Justification = "Cannot use `IRawTypedId` as generic parameter.")]
    public void Generated_class_implements_ITypedId()
    {
        Assert.IsAssignableFrom(typeof(IRawTypedId<Guid, TestGuidId>), new TestGuidId());
    }

    [Fact]
    public void Default_and_empty_are_equal()
    {
        Assert.Equal(TestGuidId.Empty, default);
    }

    [Fact]
    public void Default_value_has_empty_GUID_value()
    {
        Assert.Equal(Guid.Empty, TestGuidId.Empty.Value);
    }

    [Fact]
    public void Creating_value_out_of_guid_works()
    {
        Assert.Equal(Guid1, new TestGuidId(Guid1).Value);
    }

    [Fact]
    public void From_null_behaves_correctly()
    {
        Assert.False(TestGuidId.IsValid(null));

        Assert.Throws<FormatException>(() => TestGuidId.Parse(null));
        Assert.Null(TestGuidId.ParseNullable(null));
        Assert.False(TestGuidId.TryParse(null, out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_guid_behaves_correctly()
    {
        Assert.True(TestGuidId.IsValid(Guid1));

        Assert.Equal(new TestGuidId(Guid1), TestGuidId.Parse(Guid1));
        Assert.Equal(new TestGuidId(Guid1), TestGuidId.ParseNullable(Guid1));
        Assert.True(TestGuidId.TryParse(Guid1, out var value));
        Assert.Equal(new TestGuidId(Guid1), value);
    }

    [Fact]
    public void Equals_behaves_correctly()
    {
        Assert.True(new TestGuidId(Guid1).Equals(new TestGuidId(Guid1)));
        Assert.False(new TestGuidId(Guid1).Equals(new TestGuidId(Guid2)));
        Assert.False(new TestGuidId(Guid1).Equals(null));
    }

    [Fact]
    public void CompareTo_behaves_correctly()
    {
        Assert.Equal(0, new TestGuidId(Guid1).CompareTo(new TestGuidId(Guid1)));
        Assert.Equal(-1, new TestGuidId(Guid1).CompareTo(new TestGuidId(Guid2)));
        Assert.Equal(1, new TestGuidId(Guid2).CompareTo(new TestGuidId(Guid1)));
    }

    [Fact]
    public void Comparisons_behave_correctly()
    {
        Assert.True(new TestGuidId(Guid1) < new TestGuidId(Guid2));
        Assert.False(new TestGuidId(Guid2) < new TestGuidId(Guid2));
        Assert.False(new TestGuidId(Guid3) < new TestGuidId(Guid2));

        Assert.True(new TestGuidId(Guid1) <= new TestGuidId(Guid2));
        Assert.True(new TestGuidId(Guid2) <= new TestGuidId(Guid2));
        Assert.False(new TestGuidId(Guid3) <= new TestGuidId(Guid2));

        Assert.False(new TestGuidId(Guid1) > new TestGuidId(Guid2));
        Assert.False(new TestGuidId(Guid2) > new TestGuidId(Guid2));
        Assert.True(new TestGuidId(Guid3) > new TestGuidId(Guid2));

        Assert.False(new TestGuidId(Guid1) >= new TestGuidId(Guid2));
        Assert.True(new TestGuidId(Guid2) >= new TestGuidId(Guid2));
        Assert.True(new TestGuidId(Guid3) >= new TestGuidId(Guid2));

        Assert.True(new TestGuidId(Guid1) == new TestGuidId(Guid1));
        Assert.False(new TestGuidId(Guid1) == new TestGuidId(Guid2));
        Assert.True(new TestGuidId(Guid1) != new TestGuidId(Guid2));
        Assert.False(new TestGuidId(Guid1) != new TestGuidId(Guid1));
    }

    [Fact]
    public void The_hash_code_is_equal_to_underylying_value()
    {
        Assert.Equal(Guid1.GetHashCode(), new TestGuidId(Guid1).GetHashCode());
    }

    [Fact]
    public void Casts_to_underlying_type_extract_the_value()
    {
        Guid implicitValue = new TestGuidId(Guid1);
        var explicitValue = (Guid)new TestGuidId(Guid2);

        Assert.Equal(Guid1, implicitValue);
        Assert.Equal(Guid2, explicitValue);
    }

    [Fact]
    public void ToString_returns_string_representation_of_the_underlying_value()
    {
        Assert.Equal(Guid1.ToString(), new TestGuidId(Guid1).ToString());
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_to_from_JSON()
    {
        var value = new TestGuidId(Guid1);

        var json = JsonSerializer.Serialize(value);
        var deserialized = JsonSerializer.Deserialize<TestGuidId>(json);

        Assert.Equal(value, deserialized);
    }

    [Fact]
    public void The_type_serializes_as_raw_underlying_type()
    {
        var value = new TestGuidId(Guid1);

        var json = JsonSerializer.Serialize(value);

        Assert.Equal("\"" + Guid1 + "\"", json);
    }

    [Fact]
    public void FromDatabase_converts_data_as_Parse()
    {
        Assert.Equal(TestGuidId.FromDatabase.Compile().Invoke(Guid1), TestGuidId.Parse(Guid1));
    }

    [Fact]
    public void New_generates_random_ID()
    {
        var rnd = TestGuidId.New();
        Assert.NotEqual(rnd.Value, Guid.Empty);
    }
}
