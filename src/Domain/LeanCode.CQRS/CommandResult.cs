using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using LeanCode.CQRS.Validation;

namespace LeanCode.CQRS
{
    public class CommandResult
    {
        public ImmutableList<ValidationError> ValidationErrors { get; }
        public bool WasSuccessful => ValidationErrors.Count == 0;

        public CommandResult(IReadOnlyList<ValidationError>? validationErrors)
        {
            ValidationErrors = validationErrors is null
                ? ImmutableList.Create<ValidationError>()
                : validationErrors.ToImmutableList();
        }

        public static CommandResult Success { get; } = new CommandResult(null);

        public static CommandResult NotValid(ValidationResult validationResult)
        {
            if (validationResult.IsValid)
            {
                throw new ArgumentException(
                    "Cannot create NotValid command result if no validation errors have occurred.",
                    nameof(validationResult));
            }

            return new CommandResult(validationResult.Errors);
        }
    }
}
