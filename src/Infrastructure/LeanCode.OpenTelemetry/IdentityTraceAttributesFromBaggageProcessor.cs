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

        if (data.GetBaggageItem(IdentityTraceBaggageHelpers.InitiatorIdKey) is { } initiatorId)
        {
            data.SetTag(IdentityTraceBaggageHelpers.InitiatorIdKey, initiatorId);
        }

        var roles = data.GetUserRoleBaggage(IdentityTraceBaggageHelpers.UserRoleKey);

        if (roles is not null)
        {
            data.SetTag(IdentityTraceBaggageHelpers.UserRoleKey, roles);
        }

        var initiatorRoles = data.GetUserRoleBaggage(IdentityTraceBaggageHelpers.InitiatorRoleKey);

        if (initiatorRoles is not null)
        {
            data.SetTag(IdentityTraceBaggageHelpers.InitiatorRoleKey, initiatorRoles);
        }
    }
}
