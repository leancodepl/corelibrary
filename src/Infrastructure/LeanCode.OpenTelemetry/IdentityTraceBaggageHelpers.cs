using System.Diagnostics;
using System.Text.Json;

namespace LeanCode.OpenTelemetry;

public static class IdentityTraceBaggageHelpers
{
    public const string UserIdKey = "enduser.id";
    public const string RoleKey = "enduser.role";

    public static void SetUserRoleBaggage(this Activity activity, IEnumerable<string> roles)
    {
        var rolesJson = JsonSerializer.Serialize(roles);
        activity.AddBaggage(RoleKey, rolesJson);
    }

    public static IEnumerable<string>? GetUserRoleBaggage(this Activity activity)
    {
        var rolesJson = activity.GetBaggageItem(RoleKey);

        if (rolesJson is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(rolesJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
