using System.Diagnostics;
using System.Text.Json;

namespace LeanCode.OpenTelemetry;

public static class IdentityTraceBaggageHelpers
{
    public const string UserIdKey = "enduser.id";
    public const string UserRoleKey = "enduser.role";
    public const string InitiatorIdKey = "initiator.id";
    public const string InitiatorRoleKey = "initiator.role";

    public static void SetUserRoleBaggage(this Activity activity, string roleKey, IEnumerable<string> roles)
    {
        var rolesJson = JsonSerializer.Serialize(roles);
        activity.AddBaggage(roleKey, rolesJson);
    }

    public static IEnumerable<string>? GetUserRoleBaggage(this Activity activity, string roleKey)
    {
        var rolesJson = activity.GetBaggageItem(roleKey);

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
