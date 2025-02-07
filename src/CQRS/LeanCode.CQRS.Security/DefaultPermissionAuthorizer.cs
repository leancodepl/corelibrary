using System.Security.Claims;
using LeanCode.Contracts.Security;
using Microsoft.Extensions.Logging;

namespace LeanCode.CQRS.Security;

public class DefaultPermissionAuthorizer : CustomAuthorizer<object, string[]>, IHasPermissions
{
    private readonly ILogger<DefaultPermissionAuthorizer> logger;

    private readonly RoleRegistry registry;

    public DefaultPermissionAuthorizer(ILogger<DefaultPermissionAuthorizer> logger, RoleRegistry registry)
    {
        this.logger = logger;
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
            logger.LogWarning(
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
