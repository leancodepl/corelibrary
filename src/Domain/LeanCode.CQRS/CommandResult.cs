using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS
{
    public sealed class CommandResult
    {
        public ImmutableList<ValidationError> ValidationErrors { get; }
        public bool WasSuccessful => ValidationErrors.Count == 0;

        public CommandResult(ImmutableList<ValidationError> validationErrors)
        {
            ValidationErrors = validationErrors;
        }

        public static CommandResult Success { get; } = new(ImmutableList.Create<ValidationError>());

        public static CommandResult NotValid(ValidationResult validationResult)
        {
            if (validationResult.IsValid)
            {
                throw new ArgumentException(
                    "Cannot create NotValid command result if no validation errors have occurred.",
                    nameof(validationResult));
            }

            return new(validationResult.Errors);
        }
    }
}
