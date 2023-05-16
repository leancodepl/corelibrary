using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Ory.Kratos.Client.Model;

namespace LeanCode.Kratos;

public class KratosAuthenticationOptions : AuthenticationSchemeOptions
{
    public bool AllowInactiveIdentities { get; set; }
    public string SessionCookieName { get; set; } = KratosDefaults.SessionCookieName;
    public string NameClaimType { get; set; } = null!;
    public string RoleClaimType { get; set; } = null!;
    public Action<KratosSession, KratosAuthenticationOptions, List<Claim>> ClaimsExtractor { get; set; } = null!;
}
