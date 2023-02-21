using System.Text.Json;

namespace LeanCode.Serialization;

public class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToSnakeCase();
}
