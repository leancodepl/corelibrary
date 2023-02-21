using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;

namespace LeanCode.CQRS.Validation.Fluent;

internal class AdapterLoader<TAppContext, TCommand> : ICommandValidator<TAppContext, TCommand>
    where TAppContext : notnull
    where TCommand : ICommand
{
    private static readonly Task<ValidationResult> NoError = Task.FromResult(new ValidationResult(null));

    private readonly FluentValidationCommandValidatorAdapter<TAppContext, TCommand>? adapter;

    public AdapterLoader(IComponentContext ctx)
    {
        if (ctx.TryResolve<IValidator<TCommand>>(out var val))
        {
            adapter = new FluentValidationCommandValidatorAdapter<TAppContext, TCommand>(val, ctx);
        }
    }

    public Task<ValidationResult> ValidateAsync(TAppContext appContext, TCommand command)
    {
        return adapter is null
            ? NoError
            : adapter.ValidateAsync(appContext, command);
    }
}
