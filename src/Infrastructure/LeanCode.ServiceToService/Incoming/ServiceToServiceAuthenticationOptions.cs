using System.Collections.Frozen;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace LeanCode.ServiceToService.Incoming;

public class ServiceToServiceAuthenticationOptions : AuthenticationSchemeOptions
{
    public string NameClaimType { get; set; } = "sub";
    public string RoleClaimType { get; set; } = "role";
    public string S2SApiKey { get; set; } = "";

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "?",
        "CA2227",
        Justification = "Options binding requires a public setter."
    )]
    public FrozenDictionary<string, FrozenSet<string>> CallerRoles { get; set; } =
        FrozenDictionary<string, FrozenSet<string>>.Empty;
    public bool RejectMissingS2SApiKey { get; set; } = true;

    public bool RejectUnknownCallers { get; set; } = true;
    public bool RejectMissingCallerId { get; set; } = true;
    public bool ValidateS2SApiKeyAtStartup { get; set; } = true;

    public bool ValidateCallerRolesAtStartup { get; set; } = true;
}
