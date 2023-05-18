namespace LeanCode.CQRS.AspNetCore;

public class CQRSRequestPayload
{
    public object Payload { get; }

    /// <summary>
    /// Indicates that the <see cref="Result"/> was set and should be serialized as response.
    /// Does not mean that <see cref="Result"/> is actually non-null.
    /// </summary>
    public bool HasResult { get; private set; }
    public object? Result { get; private set; }

    public CQRSRequestPayload(object payload)
    {
        Payload = payload;
    }

    public void SetResult(object? result)
    {
        if (HasResult)
        {
            throw new InvalidOperationException("The request has been already set.");
        }

        HasResult = true;
        Result = result;
    }
}
