using LeanCode.Components;
using LeanCode.ViewRenderer;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.PdfRocket;

public class PdfRocketModuleTests
{
    [Fact]
    public void Registers_pdf_rocket_client_correctly()
    {
        var services = new ServiceCollection();

        var module = new PdfRocketModule();
        module.ConfigureServices(services);

        var config = new PdfRocketConfiguration { ApiKey = "api_key" };
        services.AddSingleton(config);
        services.AddSingleton<IViewRenderer, MockRenderer>();

        var serviceProvider = services.BuildServiceProvider();

        var pdfRocketGenerator = serviceProvider.GetService<PdfRocketGenerator>();

        Assert.NotNull(pdfRocketGenerator);
    }

    private sealed class MockRenderer : IViewRenderer
    {
        public Task RenderToStreamAsync(
            string viewName,
            object model,
            Stream outputStream,
            CancellationToken cancellationToken = default
        )
        {
            throw new System.NotImplementedException();
        }

        public Task<string> RenderToStringAsync(
            string viewName,
            object model,
            CancellationToken cancellationToken = default
        )
        {
            throw new System.NotImplementedException();
        }
    }
}
