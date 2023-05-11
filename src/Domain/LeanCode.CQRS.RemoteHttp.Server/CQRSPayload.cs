namespace LeanCode.CQRS.RemoteHttp.Server;

internal class CQRSPayload
{
    public object Object { get; }
    public object Context { get; }

    public CQRSPayload(object @object, object context)
    {
        Object = @object;
        Context = context;
    }
}
