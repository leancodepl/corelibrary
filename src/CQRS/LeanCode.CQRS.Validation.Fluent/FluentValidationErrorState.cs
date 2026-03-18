namespace LeanCode.CQRS.Validation.Fluent;

public sealed class FluentValidatorErrorState
{
    public int ErrorCode { get; }
    public string? ErrorName { get; }

    public FluentValidatorErrorState(int code, string? errorName = null)
    {
        ErrorCode = code;
        ErrorName = errorName;
    }
}
