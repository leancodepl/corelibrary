using FluentAssertions;
using LeanCode.Components;
using LeanCode.Contracts;
using LeanCode.Contracts.Security;
using LeanCode.Contracts.Validation;
using LeanCode.CQRS.AspNetCore.Local;
using LeanCode.CQRS.AspNetCore.Registration;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests.Local;

public class MiddlewaresForLocalExecutionTests
{
    private static readonly TypesCatalog ThisCatalog = TypesCatalog.Of<MiddlewaresForLocalExecutionTests>();

    [Fact]
    public async Task Tracing_middleware_works()
    {
        var executor = BuildWith(c => c.CQRSTrace());

        await executor.RunAsync(new DummyCommand(), new());
    }

    [Fact]
    public async Task Response_logging_middleware_works()
    {
        var executor = BuildWith(c => c.LogCQRSResponses());

        await executor.RunAsync(new DummyCommand(), new());
    }

    [Fact]
    public async Task Exception_translation_middleware_works()
    {
        var executor = BuildWith(c => c.TranslateExceptions());

        var result = await executor.RunAsync(new ExceptionTranslationCommand(), new());
        result.WasSuccessful.Should().BeFalse();
        result
            .ValidationErrors
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new ValidationError("", "Message", 100));
    }

    [Fact]
    public async Task Validation_middleware_works()
    {
        var executor = BuildWith(c => c.Validate());

        var result = await executor.RunAsync(new ValidatedCommand(), new());
        result.WasSuccessful.Should().BeFalse();
        result
            .ValidationErrors
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new ValidationError("", "FromValidator", 101));
    }

    [Fact]
    public async Task Security_middleware_works()
    {
        var executor = BuildWith(c => c.Secure());

        var act = () => executor.RunAsync(new SecuredCommand(), new());
        await act.Should().ThrowAsync<UnauthenticatedCQRSRequestException>();
    }

    public static ILocalCommandExecutor BuildWith(Action<ICQRSApplicationBuilder> configure)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMetrics();
        serviceCollection.AddScoped<IMiddlewareFactory>(sp => new MiddlewareFactory(sp));
        serviceCollection.AddScoped<LocalHandlerMiddleware>();
        serviceCollection.AddScoped<CQRSMetrics>();
        serviceCollection.AddScoped<ICommandValidatorResolver, CommandValidatorResolver>();

        var registrationSource = new CQRSObjectsRegistrationSource(serviceCollection, new ObjectExecutorFactory());
        registrationSource.AddCQRSObjects(ThisCatalog, ThisCatalog);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return new MiddlewareBasedLocalCommandExecutor(serviceProvider, registrationSource, configure);
    }
}

public record DummyCommand() : ICommand;

public record ExceptionTranslationCommand() : ICommand;

public record ValidatedCommand() : ICommand;

[AuthorizeWhenHasAnyOf("invalid")]
public record SecuredCommand() : ICommand;

public class DummyCommandHandler : ICommandHandler<DummyCommand>
{
    public Task ExecuteAsync(HttpContext context, DummyCommand command) => Task.CompletedTask;
}

public class ExceptionTranslationCommandHandler : ICommandHandler<ExceptionTranslationCommand>
{
    public Task ExecuteAsync(HttpContext context, ExceptionTranslationCommand command)
    {
        throw new CommandExecutionInvalidException(100, "Message");
    }
}

public class ValidatedCommandValidator : ICommandValidator<ValidatedCommand>, ICommandValidatorWrapper
{
    public Task<ValidationResult> ValidateAsync(HttpContext httpContext, ValidatedCommand command) =>
        Task.FromResult(new ValidationResult([ new ValidationError("", "FromValidator", 101) ]));

    public Task<ValidationResult> ValidateAsync(HttpContext appContext, ICommand command) =>
        ValidateAsync(appContext, (ValidatedCommand)command);
}

public class ValidatedCommandHandler : ICommandHandler<ValidatedCommand>
{
    public Task ExecuteAsync(HttpContext context, ValidatedCommand command) => Task.CompletedTask;
}

public class CommandValidatorResolver : ICommandValidatorResolver
{
    public ICommandValidatorWrapper? FindCommandValidator(Type commandType) => new ValidatedCommandValidator();
}

public class SecureCommandHandler : ICommandHandler<SecuredCommand>
{
    public Task ExecuteAsync(HttpContext context, SecuredCommand command) => Task.CompletedTask;
}
