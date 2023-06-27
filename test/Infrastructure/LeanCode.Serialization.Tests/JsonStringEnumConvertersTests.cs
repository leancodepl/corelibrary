using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace LeanCode.Serialization.Tests;

public partial class JsonStringEnumConvertersTests
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
    public enum CamelCase
    {
        ExampleValue
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Camel_case_converter_correctly_converts_enum_name(bool useSourceGenerationContext)
    {
        var result = useSourceGenerationContext
            ? JsonSerializer.Serialize(CamelCase.ExampleValue, StringEnumConvertersContext.Default.CamelCase)
            : JsonSerializer.Serialize(CamelCase.ExampleValue);

        Assert.Equal(@"""exampleValue""", result);
    }

    [JsonConverter(typeof(JsonKebabCaseLowerStringEnumConverter))]
    public enum KebabCaseLower
    {
        ExampleValue
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Kebab_case_lower_converter_correctly_converts_enum_name(bool useSourceGenerationContext)
    {
        var result = useSourceGenerationContext
            ? JsonSerializer.Serialize(KebabCaseLower.ExampleValue, StringEnumConvertersContext.Default.KebabCaseLower)
            : JsonSerializer.Serialize(KebabCaseLower.ExampleValue);

        Assert.Equal(@"""example-value""", result);
    }

    [JsonConverter(typeof(JsonKebabCaseUpperStringEnumConverter))]
    public enum KebabCaseUpper
    {
        ExampleValue
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Kebab_case_upper_converter_correctly_converts_enum_name(bool useSourceGenerationContext)
    {
        var result = useSourceGenerationContext
            ? JsonSerializer.Serialize(KebabCaseUpper.ExampleValue, StringEnumConvertersContext.Default.KebabCaseUpper)
            : JsonSerializer.Serialize(KebabCaseUpper.ExampleValue);

        Assert.Equal(@"""EXAMPLE-VALUE""", result);
    }

    [JsonConverter(typeof(JsonSnakeCaseLowerStringEnumConverter))]
    public enum SnakeCaseLower
    {
        ExampleValue
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Snake_case_lower_converter_correctly_converts_enum_name(bool useSourceGenerationContext)
    {
        var result = useSourceGenerationContext
            ? JsonSerializer.Serialize(SnakeCaseLower.ExampleValue, StringEnumConvertersContext.Default.SnakeCaseLower)
            : JsonSerializer.Serialize(SnakeCaseLower.ExampleValue);

        Assert.Equal(@"""example_value""", result);
    }

    [JsonConverter(typeof(JsonSnakeCaseUpperStringEnumConverter))]
    public enum SnakeCaseUpper
    {
        ExampleValue
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Snake_case_upper_converter_correctly_converts_enum_name(bool useSourceGenerationContext)
    {
        var result = useSourceGenerationContext
            ? JsonSerializer.Serialize(SnakeCaseUpper.ExampleValue, StringEnumConvertersContext.Default.SnakeCaseUpper)
            : JsonSerializer.Serialize(SnakeCaseUpper.ExampleValue);

        Assert.Equal(@"""EXAMPLE_VALUE""", result);
    }

    [JsonSerializable(typeof(CamelCase))]
    [JsonSerializable(typeof(KebabCaseLower))]
    [JsonSerializable(typeof(KebabCaseUpper))]
    [JsonSerializable(typeof(SnakeCaseLower))]
    [JsonSerializable(typeof(SnakeCaseUpper))]
    internal sealed partial class StringEnumConvertersContext : JsonSerializerContext { }
}
