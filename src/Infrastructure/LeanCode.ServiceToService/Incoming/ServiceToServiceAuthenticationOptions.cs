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
    public Dictionary<string, HashSet<string>> CallerRoles { get; set; } = [];

    public bool RejectUnknownCallers { get; set; } = true;
    public bool ValidateS2SApiKeyAtStartup { get; set; } = true;

    public bool ValidateCallerRolesAtStartup { get; set; } = true;
}
