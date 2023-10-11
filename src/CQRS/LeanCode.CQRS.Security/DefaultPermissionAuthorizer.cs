using System.Security.Claims;
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
        ClaimsPrincipal user,
        object obj,
        string[]? customData,
        CancellationToken cancellationToken
    )
    {
        if (!user.HasPermission(registry, customData ?? Array.Empty<string>()))
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
