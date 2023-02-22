using LeanCode.CQRS.Execution;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Serilog;
using Serilog.Events;
using Xunit;

namespace LeanCode.CQRS.Tests;

public class LoggingResponsesTests
{
    private const string Response = "Success!";

    [Fact]
    public async Task Response_logger_element_correctly_log_response()
    {
        var appCtx = new AppContext();
        var fakeSink = new FakeSink();

        var logger = new LoggerConfiguration().MinimumLevel
            .Is(LogEventLevel.Verbose)
            .WriteTo.Sink(fakeSink, LogEventLevel.Verbose)
            .CreateLogger();

        var element = new ResponseLoggerElement<AppContext, object, object>(logger);

        var result = await element.ExecuteAsync(appCtx, Response, (ctx, obj) => Task.FromResult<object>(obj));

        Assert.Single(fakeSink.Messages);
        Assert.Contains(result.ToString(), fakeSink.Messages[0]);
    }

    [Fact]
    public async Task Response_logger_element_correctly_run_next()
    {
        var appCtx = new AppContext();

        var element = new ResponseLoggerElement<AppContext, object, object>();

        var result = await element.ExecuteAsync(appCtx, Response, (ctx, obj) => Task.FromResult<object>(obj));

        Assert.Equal(Response, result);
    }

    [Fact]
    public async Task Non_Production_response_logger_element_correctly_log_response_on_Non_Production_env()
    {
        var appCtx = new AppContext();
        var fakeSink = new FakeSink();

        IHostEnvironment env = new HostingEnvironment { EnvironmentName = Environments.Development };

        var logger = new LoggerConfiguration().MinimumLevel
            .Is(LogEventLevel.Verbose)
            .WriteTo.Sink(fakeSink, LogEventLevel.Verbose)
            .CreateLogger();

        var element = new NonProductionResponseLoggerElement<AppContext, object, object>(env, logger);

        var result = await element.ExecuteAsync(appCtx, Response, (ctx, obj) => Task.FromResult<object>(obj));

        Assert.Single(fakeSink.Messages);
        Assert.Contains(result.ToString(), fakeSink.Messages[0]);
    }

    [Fact]
    public async Task Non_Production_response_logger_element_correctly_run_next_on_Non_Production_env()
    {
        var appCtx = new AppContext();

        IHostEnvironment env = new HostingEnvironment { EnvironmentName = Environments.Development };

        var element = new NonProductionResponseLoggerElement<AppContext, object, object>(env);

        var result = await element.ExecuteAsync(appCtx, Response, (ctx, obj) => Task.FromResult<object>(obj));

        Assert.Equal(Response, result);
    }

    [Fact]
    public async Task Non_Production_response_logger_element_does_not_log_response_on_prod_env()
    {
        var appCtx = new AppContext();
        var fakeSink = new FakeSink();

        IHostEnvironment env = new HostingEnvironment { EnvironmentName = Environments.Production };

        var logger = new LoggerConfiguration().MinimumLevel
            .Is(LogEventLevel.Verbose)
            .WriteTo.Sink(fakeSink, LogEventLevel.Verbose)
            .CreateLogger();

        var element = new NonProductionResponseLoggerElement<AppContext, object, object>(env, logger);

        var result = await element.ExecuteAsync(appCtx, Response, (ctx, obj) => Task.FromResult<object>(obj));

        Assert.Empty(fakeSink.Messages);
    }

    [Fact]
    public async Task Non_Production_response_logger_element_correctly_run_next_on_prod_env()
    {
        var appCtx = new AppContext();

        IHostEnvironment env = new HostingEnvironment { EnvironmentName = Environments.Production };

        var element = new NonProductionResponseLoggerElement<AppContext, object, object>(env);

        var result = await element.ExecuteAsync(appCtx, Response, (ctx, obj) => Task.FromResult<object>(obj));

        Assert.Equal(Response, result);
    }
}
