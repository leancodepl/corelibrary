using FluentValidation;
using LeanCode.Contracts;

namespace LeanCode.CodeAnalysis.Tests.TestSamples;

// Roslyn has only context of this file in tests so it's necessary to declare `AcceptedValidatorCommand` here
// to check if it implements `ICommand` interface.
public class AcceptedValidatorCommand : ICommand { }

public class AcceptedValidatorOperation : IOperation { }

public class AcceptedValidatorCommandCV : AbstractValidator<AcceptedValidatorCommand>
{
    public AcceptedValidatorCommandCV() { }
}

// Do not raise diagnostics for classes not implementing `ICommand` interface.
public class Validator : AbstractValidator<AcceptedValidatorOperation>
{
    public Validator() { }
}
