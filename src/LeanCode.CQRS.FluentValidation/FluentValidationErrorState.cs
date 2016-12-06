namespace LeanCode.CQRS.FluentValidation
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
