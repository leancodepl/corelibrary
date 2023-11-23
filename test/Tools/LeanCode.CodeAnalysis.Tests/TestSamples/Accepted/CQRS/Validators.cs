using FluentValidation;
using LeanCode.Contracts;

namespace LeanCode.CodeAnalysis.Tests.TestSamples.Accepted.CQRS;

// Roslyn has only context of this file in tests so it's necessary to declare `AcceptedValidatorCommand` here
// to check if it implements `ICommand` interface.
public class ValidatorCommand : ICommand { }

public class ValidatorOperation : IOperation { }

public class ValidatorCommandCV : AbstractValidator<ValidatorCommand>
{
    public ValidatorCommandCV() { }
}

// Do not raise diagnostics for classes not implementing `ICommand` interface.
public class Validator : AbstractValidator<ValidatorOperation>
{
    public Validator() { }
}
