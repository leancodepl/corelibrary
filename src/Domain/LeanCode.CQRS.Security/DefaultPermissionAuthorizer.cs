using System.Threading.Tasks;
using LeanCode.Contracts.Security;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Security;

public class DefaultPermissionAuthorizer : CustomAuthorizer<object, string[]>, IHasPermissions
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<DefaultPermissionAuthorizer>();

    private readonly RoleRegistry registry;

    public DefaultPermissionAuthorizer(RoleRegistry registry)
    {
        this.registry = registry;
    }

    protected override Task<bool> CheckIfAuthorizedAsync(
        HttpContext appContext,
        object obj,
        string[]? customData = null
    )
    {
        if (!appContext.User.HasPermission(registry, customData ?? System.Array.Empty<string>()))
        {
            logger.Warning(
                "User does not have sufficient permissions ({Permissions}) to run {@Object}",
                customData,
                obj
            );

            return Task.FromResult(false);
        }
        else
        {
            return Task.FromResult(true);
        }
    }
}
