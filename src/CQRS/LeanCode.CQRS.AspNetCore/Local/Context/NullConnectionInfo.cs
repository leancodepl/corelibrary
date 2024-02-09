using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class NullConnectionInfo : ConnectionInfo
{
    public static readonly NullConnectionInfo Empty = new();

    public override string Id
    {
        get => "";
        set { }
    }

    public override IPAddress? RemoteIpAddress
    {
        get => null;
        set { }
    }

    public override int RemotePort
    {
        get => 0;
        set { }
    }

    public override IPAddress? LocalIpAddress
    {
        get => null;
        set { }
    }

    public override int LocalPort
    {
        get => 0;
        set { }
    }

    public override X509Certificate2? ClientCertificate
    {
        get => null;
        set { }
    }

    private NullConnectionInfo() { }

    public override Task<X509Certificate2?> GetClientCertificateAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<X509Certificate2?>(null);
}
