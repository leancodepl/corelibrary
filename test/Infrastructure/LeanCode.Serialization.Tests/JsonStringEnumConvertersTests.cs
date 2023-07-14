using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace LeanCode.Serialization.Tests;

public partial class JsonStringEnumConvertersTests
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter<CamelCase>))]
    public enum CamelCase
    {
        ExampleValue
    }

    [Fact]
    public void Camel_case_converter_correctly_converts_enum_name()
    {
        var result = JsonSerializer.Serialize(CamelCase.ExampleValue);

        Assert.Equal(@"""exampleValue""", result);
    }

    [Fact]
    public void Camel_case_converter_correctly_converts_enum_name_when_serializing_using_context()
    {
        var result = JsonSerializer.Serialize(CamelCase.ExampleValue, StringEnumConvertersContext.Default.CamelCase);

        Assert.Equal(@"""exampleValue""", result);
    }

    [JsonConverter(typeof(JsonKebabCaseLowerStringEnumConverter<KebabCaseLower>))]
    public enum KebabCaseLower
    {
        ExampleValue
    }

    [Fact]
    public void Kebab_case_lower_converter_correctly_converts_enum_name()
    {
        var result = JsonSerializer.Serialize(KebabCaseLower.ExampleValue);

        Assert.Equal(@"""example-value""", result);
    }

    [Fact]
    public void Kebab_case_lower_converter_correctly_converts_enum_name_when_serializing_using_context()
    {
        var result = JsonSerializer.Serialize(
            KebabCaseLower.ExampleValue,
            StringEnumConvertersContext.Default.KebabCaseLower
        );

        Assert.Equal(@"""example-value""", result);
    }

    [JsonConverter(typeof(JsonKebabCaseUpperStringEnumConverter<KebabCaseUpper>))]
    public enum KebabCaseUpper
    {
        ExampleValue
    }

    [Fact]
    public void Kebab_case_upper_converter_correctly_converts_enum_name()
    {
        var result = JsonSerializer.Serialize(KebabCaseUpper.ExampleValue);

        Assert.Equal(@"""EXAMPLE-VALUE""", result);
    }

    [Fact]
    public void Kebab_case_upper_converter_correctly_converts_enum_name_when_serializing_using_context()
    {
        var result = JsonSerializer.Serialize(
            KebabCaseUpper.ExampleValue,
            StringEnumConvertersContext.Default.KebabCaseUpper
        );

        Assert.Equal(@"""EXAMPLE-VALUE""", result);
    }

    [JsonConverter(typeof(JsonSnakeCaseLowerStringEnumConverter<SnakeCaseLower>))]
    public enum SnakeCaseLower
    {
        ExampleValue
    }

    [Fact]
    public void Snake_case_lower_converter_correctly_converts_enum_name()
    {
        var result = JsonSerializer.Serialize(SnakeCaseLower.ExampleValue);

        Assert.Equal(@"""example_value""", result);
    }

    [Fact]
    public void Snake_case_lower_converter_correctly_converts_enum_name_when_serializing_using_context()
    {
        var result = JsonSerializer.Serialize(
            SnakeCaseLower.ExampleValue,
            StringEnumConvertersContext.Default.SnakeCaseLower
        );

        Assert.Equal(@"""example_value""", result);
    }

    [JsonConverter(typeof(JsonSnakeCaseUpperStringEnumConverter<SnakeCaseUpper>))]
    public enum SnakeCaseUpper
    {
        ExampleValue
    }

    [Fact]
    public void Snake_case_upper_converter_correctly_converts_enum_name()
    {
        var result = JsonSerializer.Serialize(SnakeCaseUpper.ExampleValue);

        Assert.Equal(@"""EXAMPLE_VALUE""", result);
    }

    [Fact]
    public void Snake_case_upper_converter_correctly_converts_enum_name_when_serializing_using_context()
    {
        var result = JsonSerializer.Serialize(
            SnakeCaseUpper.ExampleValue,
            StringEnumConvertersContext.Default.SnakeCaseUpper
        );

        Assert.Equal(@"""EXAMPLE_VALUE""", result);
    }

    [JsonSerializable(typeof(CamelCase))]
    [JsonSerializable(typeof(KebabCaseLower))]
    [JsonSerializable(typeof(KebabCaseUpper))]
    [JsonSerializable(typeof(SnakeCaseLower))]
    [JsonSerializable(typeof(SnakeCaseUpper))]
    internal sealed partial class StringEnumConvertersContext : JsonSerializerContext { }
}
