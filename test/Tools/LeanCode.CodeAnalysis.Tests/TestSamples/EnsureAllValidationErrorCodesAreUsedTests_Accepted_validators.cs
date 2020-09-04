using LeanCode.CQRS;
using LeanCode.CQRS.Validation.Fluent;
using FluentValidation;

namespace LeanCode.CodeAnalysis.Tests.TestSamples
{
    public class NestedCodes
    {
        public const int FieldTooLong = 2;
    }

    public class Command : ICommand
    {
        public string Field { get; set; }

        public static class ErrorCodes
        {
            public const int FieldNull = 1;
            public sealed class Nested : NestedCodes { }
        }
    }

    public class Validator : ContextualValidator<Command>
    {
        public Validator()
        {
            RuleFor(cmd => cmd.Field)
                .NotNull()
                    .WithCode(Command.ErrorCodes.FieldNull)
                .MaximumLength(10)
                    .WithCode(Command.ErrorCodes.Nested.FieldTooLong);
        }
    }
}
