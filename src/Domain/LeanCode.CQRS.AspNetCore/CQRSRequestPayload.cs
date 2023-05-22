namespace LeanCode.CQRS.AspNetCore;

public class CQRSRequestPayload
{
    public object Payload { get; }

    public ExecutionResult? Result { get; private set; }

    public CQRSRequestPayload(object payload)
    {
        Payload = payload;
    }

    public void SetResult(ExecutionResult? result)
    {
        if (Result is not null)
        {
            throw new InvalidOperationException("The request has been already set.");
        }

        Result = result;
    }
}
