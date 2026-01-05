using System.Text.Json;

namespace LeanCode.Serialization;

public static class CQRSJsonSerializerOptionsExtensions
{
    public static void ConfigureForCQRS(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonLaxDateOnlyConverter());
        options.Converters.Add(new JsonLaxTimeOnlyConverter());
        options.Converters.Add(new JsonLaxDateTimeOffsetConverter());
        options.PropertyNamingPolicy = null;
    }
}
