namespace LeanCode.CQRS.Execution;

public readonly record struct ExecutionResult
{
    public int StatusCode { get; private init; }

    /// <remarks>
    /// Indicates that result should be written in http response, however <see cref="Payload"/> may still be a null value.
    /// </remarks>
    public bool HasPayload { get; private init; }
    public object? Payload { get; private init; }

    public static ExecutionResult Empty(int code) => new() { StatusCode = code, };

    public static ExecutionResult WithPayload(object? payload, int code = 200) =>
        new()
        {
            StatusCode = code,
            HasPayload = true,
            Payload = payload,
        };
}
