using System;
using System.Collections.Generic;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS
{
    public class CommandResult
    {
        public IReadOnlyList<ValidationError> ValidationErrors { get; }
        public bool WasSuccessful => ValidationErrors.Count == 0;

        private CommandResult(IReadOnlyList<ValidationError> validationErrors)
        {
            this.ValidationErrors = validationErrors ?? new ValidationError[0];
        }

        public static CommandResult Success()
        {
            return new CommandResult(null);
        }

        public static CommandResult NotValid(ValidationResult validationResult)
        {
            if (validationResult == null) throw new ArgumentNullException(nameof(validationResult));
            if (validationResult.Errors == null || validationResult.Errors.Count == 0)
                throw new ArgumentException("Cannot create NotValid command result if no validation errors have occurred.", nameof(validationResult));

            return new CommandResult(validationResult.Errors);
        }
    }
}
