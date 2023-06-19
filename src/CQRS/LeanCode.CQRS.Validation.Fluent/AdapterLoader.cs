using FluentValidation;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.CQRS.Validation.Fluent;

internal sealed class AdapterLoader<TCommand> : ICommandValidator<TCommand>
    where TCommand : ICommand
{
    private readonly IServiceProvider serviceProvider;
    private static readonly Task<ValidationResult> NoError = Task.FromResult(new ValidationResult(null));

    public AdapterLoader(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public Task<ValidationResult> ValidateAsync(HttpContext httpContext, TCommand command)
    {
        var val = serviceProvider.GetService<IValidator<TCommand>>();

        if (val is null)
        {
            return NoError;
        }

        var adapter = new FluentValidationCommandValidatorAdapter<TCommand>(val);

        return adapter.ValidateAsync(httpContext, command);
    }
}
