using System;
using System.Collections.Generic;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS
{
    /// <summary> Represents a result of a command execution </summary>
    public class CommandResult
    {
        private static readonly ValidationError[] EmptyErrors = new ValidationError[0];

        public IReadOnlyList<ValidationError> ValidationErrors { get; }

        /// <value><c>true</c> if there was no validation errors, <c>false</c> otherwise </value>
        public bool WasSuccessful => ValidationErrors.Count == 0;

        public CommandResult(IReadOnlyList<ValidationError> validationErrors)
        {
            this.ValidationErrors = validationErrors ?? EmptyErrors;
        }

        public static CommandResult Success()
        {
            return new CommandResult(null);
        }

        public static CommandResult NotValid(ValidationResult validationResult)
        {
            if (validationResult == null)
            {
                throw new ArgumentNullException(nameof(validationResult));
            }
            if (validationResult.Errors == null || validationResult.Errors.Count == 0)
            {
                throw new ArgumentException(
                    "Cannot create NotValid command result if no validation errors have occurred.",
                    nameof(validationResult));
            }

            return new CommandResult(validationResult.Errors);
        }
    }
}
