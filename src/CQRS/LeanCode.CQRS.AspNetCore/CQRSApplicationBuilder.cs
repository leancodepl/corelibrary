using LeanCode.CQRS.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.AspNetCore;

public interface ICQRSApplicationBuilder : IApplicationBuilder { }

internal class CQRSApplicationBuilder : ICQRSApplicationBuilder
{
    private readonly IApplicationBuilder builder;

    public CQRSApplicationBuilder(IApplicationBuilder builder)
    {
        this.builder = builder;
    }

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) => builder.Use(middleware);

    public IApplicationBuilder New() => builder.New();

    public RequestDelegate Build() => builder.Build();

    public IServiceProvider ApplicationServices
    {
        get => builder.ApplicationServices;
        set => builder.ApplicationServices = value;
    }

    public IFeatureCollection ServerFeatures => builder.ServerFeatures;

    public IDictionary<string, object?> Properties => builder.Properties;
}

public static class CQRSApplicationBuilderExtensions
{
    public static ICQRSApplicationBuilder Validate(this ICQRSApplicationBuilder builder)
    {
        builder.UseMiddleware<CQRSValidationMiddleware>();
        return builder;
    }

    public static ICQRSApplicationBuilder Secure(this ICQRSApplicationBuilder builder)
    {
        builder.UseMiddleware<CQRSSecurityMiddleware>();
        return builder;
    }

    public static ICQRSApplicationBuilder LogCQRSResponsesOnNonProduction(this ICQRSApplicationBuilder builder)
    {
        builder.UseMiddleware<NonProductionResponseLoggerMiddleware>();
        return builder;
    }

    public static ICQRSApplicationBuilder LogCQRSResponses(this ICQRSApplicationBuilder builder)
    {
        builder.UseMiddleware<ResponseLoggerMiddleware>();
        return builder;
    }

    public static ICQRSApplicationBuilder CQRSTrace(this ICQRSApplicationBuilder builder)
    {
        builder.UseMiddleware<CQRSTracingMiddleware>();
        return builder;
    }

    public static ICQRSApplicationBuilder TranslateExceptions(this ICQRSApplicationBuilder builder)
    {
        builder.UseMiddleware<CQRSExceptionTranslationMiddleware>();
        return builder;
    }
}
