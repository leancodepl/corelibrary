using System.Collections.Generic;

namespace LeanCode.CQRS.Validation
{
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public IReadOnlyList<ValidationError> Errors { get; }

        public ValidationResult(IReadOnlyList<ValidationError> errors)
        {
            Errors = errors ?? new ValidationError[0];
        }
    }
}
