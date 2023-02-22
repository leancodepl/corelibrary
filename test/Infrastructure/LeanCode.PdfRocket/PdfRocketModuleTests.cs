using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LeanCode.ViewRenderer;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.PdfRocket;

public class PdfRocketModuleTests
{
    [Fact]
    public void Registers_pdf_rocket_client_correctly()
    {
        var builder = new ContainerBuilder();
        var services = new ServiceCollection();

        var module = new PdfRocketModule();
        builder.RegisterModule(module);
        module.ConfigureServices(services);
        builder.Populate(services);

        var config = new PdfRocketConfiguration { ApiKey = "api_key" };
        builder.RegisterInstance(config).AsSelf();
        builder.RegisterType<MockRenderer>().AsImplementedInterfaces();

        var container = builder.Build();

        var succ = container.TryResolve<PdfRocketGenerator>(out _);

        Assert.True(succ);
    }

    private class MockRenderer : IViewRenderer
    {
        public Task RenderToStreamAsync(string viewName, object model, Stream outputStream, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> RenderToStringAsync(string viewName, object model, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
