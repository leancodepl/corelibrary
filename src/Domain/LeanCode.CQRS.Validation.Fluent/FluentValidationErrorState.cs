namespace LeanCode.CQRS.Validation.Fluent
{
    public sealed class FluentValidatorErrorState
    {
        public int ErrorCode { get; }

        public FluentValidatorErrorState(int code)
        {
            this.ErrorCode = code;
        }
    }
}
