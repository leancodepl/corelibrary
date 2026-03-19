using LeanCode.CQRS.Security;
using Microsoft.Extensions.Options;

namespace LeanCode.ServiceToService.Incoming;

public class ServiceToServiceCallerRolesValidator(RoleRegistry roleRegistry)
    : IValidateOptions<ServiceToServiceAuthenticationOptions>
{
    public ValidateOptionsResult Validate(string? name, ServiceToServiceAuthenticationOptions options)
    {
        if (!options.ValidateCallerRolesAtStartup && !options.ValidateS2SApiKeyAtStartup)
        {
            return ValidateOptionsResult.Skip;
        }

        var errors = new List<string>();

        if (options.ValidateS2SApiKeyAtStartup && string.IsNullOrWhiteSpace(options.S2SApiKey))
        {
            errors.Add("S2SApiKey must be configured when ValidateS2SApiKeyAtStartup is enabled.");
        }

        if (options.ValidateCallerRolesAtStartup)
        {
            var availableRoles = roleRegistry.All.Select(role => role.Name).ToHashSet(StringComparer.Ordinal);
            var unknownRoles = options
                .CallerRoles.Values.SelectMany(roles => roles)
                .Distinct(StringComparer.Ordinal)
                .Except(availableRoles, StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            if (unknownRoles.Length > 0)
            {
                errors.Add($"Unknown role names configured in CallerRoles: {string.Join(", ", unknownRoles)}.");
            }
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
