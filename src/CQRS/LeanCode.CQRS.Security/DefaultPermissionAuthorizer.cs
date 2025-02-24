using System.Security.Claims;
using LeanCode.Contracts.Security;

namespace LeanCode.CQRS.Security;

public class DefaultPermissionAuthorizer : CustomAuthorizer<object, string[]>, IHasPermissions
{
    private readonly Serilog.ILogger logger;

    private readonly RoleRegistry registry;

    public DefaultPermissionAuthorizer(Serilog.ILogger logger, RoleRegistry registry)
    {
        this.logger = logger.ForContext<DefaultPermissionAuthorizer>();
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
