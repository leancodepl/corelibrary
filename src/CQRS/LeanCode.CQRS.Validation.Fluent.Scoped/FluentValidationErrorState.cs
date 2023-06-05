namespace LeanCode.CQRS.Validation.Fluent.Scoped;

public sealed class FluentValidatorErrorState
{
    public int ErrorCode { get; }

    public FluentValidatorErrorState(int code)
    {
        ErrorCode = code;
    }
}
