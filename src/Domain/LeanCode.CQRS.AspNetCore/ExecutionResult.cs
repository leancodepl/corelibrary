namespace LeanCode.CQRS.AspNetCore;

public struct ExecutionResult
{
    public int StatusCode { get; private set; }

    /// <remarks>
    /// Indicates that result should be written in http response, however <see cref="Payload"/> may still be a null value.
    /// </remarks>
    public bool HasPayload { get; private set; }
    public object? Payload { get; private set; }

    public static ExecutionResult Empty(int code) => new() { StatusCode = code, };

    public static ExecutionResult WithPayload(object? payload, int code = 200) =>
        new()
        {
            StatusCode = code,
            HasPayload = true,
            Payload = payload,
        };
}
