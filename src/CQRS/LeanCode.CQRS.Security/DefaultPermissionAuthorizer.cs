using System.Security.Claims;
using LeanCode.Contracts.Security;
using LeanCode.Logging;

namespace LeanCode.CQRS.Security;

public class DefaultPermissionAuthorizer : CustomAuthorizer<object, string[]>, IHasPermissions
{
    private readonly RoleRegistry registry;
    private readonly ILogger<DefaultPermissionAuthorizer> logger;

    public DefaultPermissionAuthorizer(RoleRegistry registry, ILogger<DefaultPermissionAuthorizer> logger)
    {
        this.registry = registry;
        this.logger = logger;
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
