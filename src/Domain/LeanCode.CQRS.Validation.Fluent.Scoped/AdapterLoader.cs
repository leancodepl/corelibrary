using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Registration;
using FluentValidation;
using LeanCode.Contracts;
using LeanCode.Contracts.Validation;

namespace LeanCode.CQRS.Validation.Fluent.Scoped;

internal sealed class AdapterLoader<TAppContext, TCommand> : ICommandValidator<TAppContext, TCommand>
    where TAppContext : notnull
    where TCommand : ICommand
{
    private static readonly Task<ValidationResult> NoError = Task.FromResult(new ValidationResult(null));
    private IComponentContext ctx;

    public AdapterLoader(IComponentContext ctx)
    {
        this.ctx = ctx;
    }

    public Task<ValidationResult> ValidateAsync(TAppContext appContext, TCommand command)
    {
        var val = ctx.ResolveOptional<IValidator<TCommand>>(new TypedParameter(typeof(TAppContext), appContext));

        if (val is null)
        {
            return NoError;
        }

        var adapter = new FluentValidationCommandValidatorAdapter<TAppContext, TCommand>(val);

        return adapter.ValidateAsync(appContext, command);
    }
}
