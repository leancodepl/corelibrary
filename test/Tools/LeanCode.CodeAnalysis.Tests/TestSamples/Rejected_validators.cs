using FluentValidation;
using LeanCode.Contracts;

namespace LeanCode.CodeAnalysis.Tests.TestSamples;

// Roslyn has only context of this file in tests so it's necessary to declare `RejectedValidatorCommand` here
// to check if it implements `ICommand` interface.
public class RejectedValidatorCommand : ICommand { }

public class WrongName : AbstractValidator<RejectedValidatorCommand>
{
    public WrongName() { }
}
