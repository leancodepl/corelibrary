namespace LeanCode.CQRS.Execution;
/// <summary>
/// Use to propagate arbitrary <see cref="LeanCode.Contracts.Validation.ValidationError"/> from the command handler.
/// Use as a last resort - when validation and execution are so coupled that separating it is too complicated.
/// </summary>
public class CommandExecutionInvalidException : Exception
{
    public int ErrorCode { get; private set; }

    public CommandExecutionInvalidException(int errorCode, string errorMessage, Exception? innerException = null)
        : base(errorMessage, innerException)
    {
        ErrorCode = errorCode;
    }
}
