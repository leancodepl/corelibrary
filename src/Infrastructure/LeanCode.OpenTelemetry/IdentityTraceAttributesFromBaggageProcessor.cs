using System.Diagnostics;
using OpenTelemetry;

namespace LeanCode.OpenTelemetry;

public class IdentityTraceAttributesFromBaggageProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        if (activity.GetBaggageItem(IdentityTraceBaggageHelpers.UserIdKey) is string userId)
        {
            activity.SetTag(IdentityTraceBaggageHelpers.UserIdKey, userId);
        }

        var roles = activity.GetUserRoleBaggage();

        if (roles is not null)
        {
            activity.SetTag(IdentityTraceBaggageHelpers.RoleKey, roles);
        }
    }
}
