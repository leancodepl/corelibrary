using System.Diagnostics;
using OpenTelemetry;

namespace LeanCode.OpenTelemetry;

public class IdentityTraceAttributesFromBaggageProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity data)
    {
        if (data.GetBaggageItem(IdentityTraceBaggageHelpers.UserIdKey) is { } userId)
        {
            data.SetTag(IdentityTraceBaggageHelpers.UserIdKey, userId);
        }

        var roles = data.GetUserRoleBaggage();

        if (roles is not null)
        {
            data.SetTag(IdentityTraceBaggageHelpers.RoleKey, roles);
        }
    }
}
