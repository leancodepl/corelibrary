namespace LeanCode.CQRS.AspNetCore;

public class CQRSRequestPayload
{
    public object Context { get; }
    public object Payload { get; }
    public object? Result { get; private set; }

    public CQRSRequestPayload(object context, object payload)
    {
        Context = context;
        Payload = payload;
    }

    public void SetResult(object? result)
    {
        Result = result;
    }
}
