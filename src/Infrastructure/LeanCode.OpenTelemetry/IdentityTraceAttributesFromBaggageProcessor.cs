using System.Diagnostics;
using OpenTelemetry;

namespace LeanCode.OpenTelemetry;

public class IdentityTraceAttributesFromBaggageProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity data)
    {
        if (data.GetBaggageItem(IdentityTraceBaggageHelpers.CurrentUserIdKey) is { } userId)
        {
            data.SetTag(IdentityTraceBaggageHelpers.CurrentUserIdKey, userId);
        }

        if (data.GetBaggageItem(IdentityTraceBaggageHelpers.EndUserIdKey) is { } initiatorId)
        {
            data.SetTag(IdentityTraceBaggageHelpers.EndUserIdKey, initiatorId);
        }

        var roles = data.GetUserRoleBaggage(IdentityTraceBaggageHelpers.CurrentUserRoleKey);

        if (roles is not null)
        {
            data.SetTag(IdentityTraceBaggageHelpers.CurrentUserRoleKey, roles);
        }

        var initiatorRoles = data.GetUserRoleBaggage(IdentityTraceBaggageHelpers.EndUserRoleKey);

        if (initiatorRoles is not null)
        {
            data.SetTag(IdentityTraceBaggageHelpers.EndUserRoleKey, initiatorRoles);
        }
    }
}
