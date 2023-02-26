using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using LeanCode.DomainModels.Ids;
using Xunit;

namespace LeanCode.DomainModels.Tests.Ids;

[TypedId(TypedIdFormat.RawLong)]
public readonly partial record struct TestLongId;

[TypedId(TypedIdFormat.RawLong, CustomGenerator = "5")]
public readonly partial record struct CustomGenLongId;

public class LongIdTests
{
    [Fact]
    [SuppressMessage("?", "xUnit2007", Justification = "Cannot use `IRawTypedId` as generic parameter.")]
    public void Generated_class_implements_ITypedId()
    {
        Assert.IsAssignableFrom(typeof(IRawTypedId<long, TestLongId>), new TestLongId());
    }

    [Fact]
    public void Default_and_empty_are_equal()
    {
        Assert.Equal(TestLongId.Empty, default);
    }

    [Fact]
    public void Default_value_has_0_value()
    {
        Assert.Equal(0, TestLongId.Empty.Value);
    }

    [Fact]
    public void From_null_behaves_correctly()
    {
        Assert.False(TestLongId.IsValid(null));

        Assert.Throws<FormatException>(() => TestLongId.Parse(null));
        Assert.Null(TestLongId.ParseNullable(null));
        Assert.False(TestLongId.TryParse(null, out var value));
        Assert.Equal(value, default);
    }

    [Fact]
    public void From_number_behaves_correctly()
    {
        Assert.True(TestLongId.IsValid(789));

        Assert.Equal(123, new TestLongId(123).Value);
        Assert.Equal(new TestLongId(321), TestLongId.Parse(321));
        Assert.Equal(new TestLongId(321), TestLongId.ParseNullable(321));
        Assert.True(TestLongId.TryParse(456, out var value));
        Assert.Equal(new TestLongId(456), value);
    }

    [Fact]
    public void Equals_behaves_correctly()
    {
        Assert.True(new TestLongId(123).Equals(new TestLongId(123)));
        Assert.False(new TestLongId(321).Equals(new TestLongId(123)));
        Assert.False(new TestLongId(321).Equals(null));
    }

    [Fact]
    public void CompareTo_behaves_correctly()
    {
        Assert.Equal(0, new TestLongId(789).CompareTo(new TestLongId(789)));
        Assert.Equal(-1, new TestLongId(788).CompareTo(new TestLongId(789)));
        Assert.Equal(1, new TestLongId(788).CompareTo(new TestLongId(787)));
    }

    [Fact]
    public void Comparisons_behave_correctly()
    {
        Assert.True(new TestLongId(1) < new TestLongId(2));
        Assert.False(new TestLongId(2) < new TestLongId(2));
        Assert.False(new TestLongId(3) < new TestLongId(2));

        Assert.True(new TestLongId(1) <= new TestLongId(2));
        Assert.True(new TestLongId(2) <= new TestLongId(2));
        Assert.False(new TestLongId(3) <= new TestLongId(2));

        Assert.False(new TestLongId(1) > new TestLongId(2));
        Assert.False(new TestLongId(2) > new TestLongId(2));
        Assert.True(new TestLongId(3) > new TestLongId(2));

        Assert.False(new TestLongId(1) >= new TestLongId(2));
        Assert.True(new TestLongId(2) >= new TestLongId(2));
        Assert.True(new TestLongId(3) >= new TestLongId(2));
    }

    [Fact]
    public void The_hash_code_is_equal_to_underylying_value()
    {
        Assert.Equal(123.GetHashCode(), new TestLongId(123).GetHashCode());
    }

    [Fact]
    public void Casts_to_underlying_type_extract_the_value()
    {
        long implicitValue = new TestLongId(123);
        var explicitValue = (long)new TestLongId(321);

        Assert.Equal(123, implicitValue);
        Assert.Equal(321, explicitValue);
    }

    [Fact]
    public void ToString_returns_string_representation_of_the_underlying_value()
    {
        Assert.Equal("345", new TestLongId(345).ToString());
    }

    [Fact]
    public void The_type_can_be_serialized_and_deserialized_to_from_JSON()
    {
        var value = new TestLongId(1234);

        var json = JsonSerializer.Serialize(value);
        var deserialized = JsonSerializer.Deserialize<TestLongId>(json);

        Assert.Equal(value, deserialized);
    }

    [Fact]
    public void The_type_serializes_as_raw_underlying_type()
    {
        var value = new TestLongId(1234);

        var json = JsonSerializer.Serialize(value);

        Assert.Equal("1234", json);
    }

    [Fact]
    public void FromDatabase_converts_data_as_Parse()
    {
        Assert.Equal(TestLongId.FromDatabase.Compile().Invoke(1), TestLongId.Parse(1));
    }

    [Fact]
    public void New_generates_ID_using_generator_if_provided()
    {
        var rnd = CustomGenLongId.New();
        Assert.Equal(5, rnd.Value);
    }
}
