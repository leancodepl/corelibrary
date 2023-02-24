using System;
using LeanCode.Localization.StringLocalizers;
using Xunit;
using static System.Globalization.CultureInfo;

namespace LeanCode.Localization.Tests;

public class ResourceManagerStringLocalizerTests
{
    private readonly ResourceManagerStringLocalizer stringLocalizer;

    public ResourceManagerStringLocalizerTests()
    {
        stringLocalizer = new ResourceManagerStringLocalizer(
            new LocalizationConfiguration(resourceSource: typeof(ResourceManagerStringLocalizerTests))
        );
    }

    [Fact]
    public void Correct_string_is_returned_for_InvariantCulture()
    {
        var value = stringLocalizer[InvariantCulture, "order.simple"];

        Assert.Equal("Order", value);
    }

    [Fact]
    public void Correct_string_is_returned_for_specific_culture()
    {
        var value = stringLocalizer[GetCultureInfo("pl"), "order.simple"];

        Assert.Equal("Zamówienie", value);
    }

    [Fact]
    public void Correct_string_is_returned_for_more_specific_culture()
    {
        var value = stringLocalizer[GetCultureInfo("pl-PL"), "order.simple"];

        Assert.Equal("Zamówienie", value);
    }

    [Fact]
    public void Correct_string_is_returned_for_another_more_specific_culture()
    {
        var value = stringLocalizer[GetCultureInfo("pl_pl"), "order.simple"];

        Assert.Equal("Zamówienie", value);
    }

    [Fact]
    public void Default_string_is_returned_for_missing_culture()
    {
        var value = stringLocalizer[GetCultureInfo("es"), "order.simple"];

        Assert.Equal("Order", value);
    }

    [Fact]
    public void Exception_is_thrown_when_resource_is_missing()
    {
        Assert.Throws<LocalizedResourceNotFoundException>(() =>
        {
            _ = stringLocalizer[InvariantCulture, "missing"];
        });
    }
}
