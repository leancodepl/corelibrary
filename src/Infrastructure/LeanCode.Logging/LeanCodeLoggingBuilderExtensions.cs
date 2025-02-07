using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeanCode.Logging;

public static class LeanCodeLoggingBuilderExtensions
{
    public static ILoggingBuilder AddContextualLeanCodeLogger(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton(typeof(ILogger<>), typeof(ContextualLogger<>));

        return builder;
    }

    public static ILoggingBuilder AddNullLeanCodeLogger(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        return builder;
    }
}
