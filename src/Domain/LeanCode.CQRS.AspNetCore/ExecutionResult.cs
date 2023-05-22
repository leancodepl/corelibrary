namespace LeanCode.CQRS.AspNetCore;

public struct ExecutionResult
{
    public ExecutionStatus Status { get; private set; }
    public int StatusCode { get; private set; }
    public object? Payload { get; private set; }

    public bool Failed => Status == ExecutionStatus.Failed;
    public bool Succeeded => Status == ExecutionStatus.Succeeded;

    public static ExecutionResult Fail(int code) => new() { Status = ExecutionStatus.Failed, StatusCode = code, };

    public static ExecutionResult Success(object? payload, int code = 200) =>
        new()
        {
            Status = ExecutionStatus.Succeeded,
            StatusCode = code,
            Payload = payload,
        };

    public enum ExecutionStatus
    {
        Failed,
        Succeeded,
    }
}
