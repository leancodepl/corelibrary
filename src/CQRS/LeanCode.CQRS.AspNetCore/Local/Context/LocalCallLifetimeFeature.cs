using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.AspNetCore.Local.Context;

internal class LocalCallLifetimeFeature : IHttpRequestLifetimeFeature, IDisposable
{
    private readonly CancellationTokenSource source;
    private CancellationToken requestAborted;

    public CancellationToken RequestAborted
    {
        get => requestAborted;
        set => requestAborted = value;
    }

    public CancellationToken CallAborted => source.Token;

    public LocalCallLifetimeFeature(CancellationToken cancellationToken)
    {
        source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        requestAborted = source.Token;
    }

    public void Abort() => source.Cancel();

    public void Dispose() => source.Dispose();
}
