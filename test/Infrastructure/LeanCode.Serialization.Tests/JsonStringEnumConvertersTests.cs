using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace LeanCode.Serialization.Tests;

public class JsonStringEnumConvertersTests
{
    [JsonConverter(typeof(JsonCamelCaseStringEnumConverter))]
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

    [JsonConverter(typeof(JsonKebabCaseLowerStringEnumConverter))]
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

    [JsonConverter(typeof(JsonKebabCaseUpperStringEnumConverter))]
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

    [JsonConverter(typeof(JsonSnakeCaseLowerStringEnumConverter))]
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

    [JsonConverter(typeof(JsonSnakeCaseUpperStringEnumConverter))]
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
}
