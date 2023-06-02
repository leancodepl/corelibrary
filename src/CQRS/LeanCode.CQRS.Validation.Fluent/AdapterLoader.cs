using FluentValidation;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.Validation.Fluent;

internal sealed class AdapterLoader<TCommand> : ICommandValidator<TCommand>
    where TCommand : ICommand
{
    private static readonly Task<ValidationResult> NoError = Task.FromResult(new ValidationResult(null));

    private readonly FluentValidationCommandValidatorAdapter<TCommand>? adapter;

    public AdapterLoader(IServiceProvider serviceProvider)
    {
        var val = serviceProvider.GetService<IValidator<TCommand>>();

        if (val is not null)
        {
            adapter = new FluentValidationCommandValidatorAdapter<TCommand>(val);
        }
    }

    public Task<ValidationResult> ValidateAsync(HttpContext httpContext, TCommand command)
    {
        return adapter is null ? NoError : adapter.ValidateAsync(httpContext, command);
    }
}
