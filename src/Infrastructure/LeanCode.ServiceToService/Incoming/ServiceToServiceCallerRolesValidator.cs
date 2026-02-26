using LeanCode.CQRS.Security;
using Microsoft.Extensions.Options;

namespace LeanCode.ServiceToService.Incoming;

public class ServiceToServiceCallerRolesValidator(RoleRegistry roleRegistry)
    : IValidateOptions<ServiceToServiceAuthenticationOptions>
{
    public ValidateOptionsResult Validate(string? name, ServiceToServiceAuthenticationOptions options)
    {
        if (!options.ValidateCallerRolesAtStartup)
        {
            return ValidateOptionsResult.Skip;
        }

        var availableRoles = roleRegistry.All.Select(role => role.Name).ToHashSet(StringComparer.Ordinal);
        var unknownRoles = options
            .CallerRoles.Values.SelectMany(roles => roles)
            .Distinct(StringComparer.Ordinal)
            .Where(role => !availableRoles.Contains(role))
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (unknownRoles.Length == 0)
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail(
            $"Unknown role names configured in CallerRoles: {string.Join(", ", unknownRoles)}."
        );
    }
}
