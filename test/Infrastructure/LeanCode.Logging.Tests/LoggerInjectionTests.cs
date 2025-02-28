using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace LeanCode.Logging.Tests;

public class LoggerInjectionTests
{
    private readonly IServiceProvider serviceProvider;

    public LoggerInjectionTests()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureDefaultLogging(
                "test",
                new[] { typeof(LoggerInjectionTests).Assembly },
                preserveStaticLogger: true
            );

        var host = hostBuilder.Build();

        serviceProvider = host.Services;
    }

    [Fact]
    public void Logger_is_correctly_added_to_DI_container()
    {
        var logger = serviceProvider.GetService<ILogger<LoggerInjectionTests>>();

        Assert.NotNull(logger);
    }

    [Fact]
    public void Injected_logger_produces_same_output_as_instanced_logger()
    {
        var injectedLogger = serviceProvider.GetRequiredService<ILogger<LoggerInjectionTests>>();
        var instancedLogger = Serilog.Log.ForContext<LoggerInjectionTests>();
        var testClass = new Test();

        var injectedLogOutput = GetLogOutput(() => injectedLogger.Information("Test: {@TestClass}", testClass));
        var instancedLogOutput = GetLogOutput(() => instancedLogger.Information("Test: {@TestClass}", testClass));

        var injectedLogOutputWithoutTimespan = RemoveTimestamp(injectedLogOutput);
        var instancedOutputWithoutTimespan = RemoveTimestamp(instancedLogOutput);

        Assert.Equal(injectedLogOutputWithoutTimespan, instancedOutputWithoutTimespan);
    }

    private static string GetLogOutput(Action log)
    {
        using var stringWriter = new StringWriter();

        Console.SetOut(stringWriter);

        log();

        return stringWriter.ToString();
    }

    private static string RemoveTimestamp(string log)
    {
        var logJson = JsonDocument.Parse(log).RootElement;

        var logWithoutTimestamp = logJson
            .EnumerateObject()
            .Where(e => e.Name != "@t")
            .ToDictionary(e => e.Name, e => e.Value);

        return JsonSerializer.Serialize(logWithoutTimestamp);
    }

    private sealed class Test
    {
        public string Property { get; set; } = "test";
    }
}
