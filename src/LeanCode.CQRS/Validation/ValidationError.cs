namespace LeanCode.CQRS.Validation
{
    public class ValidationError
    {
        public string PropertyName { get; }
        public string ErrorMessage { get; }
        public object AttemptedValue { get; }
        public int ErrorCode { get; }

        public ValidationError(string propertyName, string errorMessage, int errorCode, object attemptedValue = null)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            AttemptedValue = attemptedValue;
        }
    }
}
