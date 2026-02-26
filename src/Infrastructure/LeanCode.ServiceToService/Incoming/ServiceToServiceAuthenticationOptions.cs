using System.Collections.Frozen;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace LeanCode.ServiceToService.Incoming;

public class ServiceToServiceAuthenticationOptions : AuthenticationSchemeOptions
{
    public string NameClaimType { get; set; } = "sub";
    public string RoleClaimType { get; set; } = "role";
    public FrozenDictionary<string, FrozenSet<string>> CallerRoles { get; set; } =
        FrozenDictionary<string, FrozenSet<string>>.Empty;
    public bool RejectUnknownCallers { get; set; } = true;
    public bool RejectMissingCallerId { get; set; } = true;
    public bool ValidateCallerRolesAtStartup { get; set; } = true;
}
