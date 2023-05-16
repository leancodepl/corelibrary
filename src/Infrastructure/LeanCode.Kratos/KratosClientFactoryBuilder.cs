using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ory.Kratos.Client.Api;
using Ory.Kratos.Client.Client;

namespace LeanCode.Kratos;

public sealed class KratosClientFactoryBuilder
{
    private readonly IServiceCollection serviceCollection;

    public KratosClientFactoryBuilder(IServiceCollection serviceCollection)
    {
        this.serviceCollection = serviceCollection;
    }

    public void AddCourierApiClient(Configuration configuration) =>
        serviceCollection.TryAddTransient<ICourierApi>(sp => new CourierApi(configuration));

    public void AddCourierApiClient(string adminBasePath) =>
        serviceCollection.TryAddTransient<ICourierApi>(sp => new CourierApi(adminBasePath));

    public void AddCourierApiClient(Uri adminBasePath) => AddCourierApiClient(adminBasePath.ToString());

    public void AddFrontendApiClient(Configuration configuration) =>
        serviceCollection.TryAddTransient<IFrontendApi>(sp => new FrontendApi(configuration));

    public void AddFrontendApiClient(string publicBasePath) =>
        serviceCollection.TryAddTransient<IFrontendApi>(sp => new FrontendApi(publicBasePath));

    public void AddFrontendApiClient(Uri publicBasePath) => AddFrontendApiClient(publicBasePath.ToString());

    public void AddIdentityApiClient(Configuration configuration) =>
        serviceCollection.TryAddTransient<IIdentityApi>(sp => new IdentityApi(configuration));

    public void AddIdentityApiClient(string adminBasePath) =>
        serviceCollection.TryAddTransient<IIdentityApi>(sp => new IdentityApi(adminBasePath));

    public void AddIdentityApiClient(Uri adminBasePath) => AddIdentityApiClient(adminBasePath.ToString());

    public void AddMetadataApiClient(Configuration configuration) =>
        serviceCollection.TryAddTransient<IMetadataApi>(sp => new MetadataApi(configuration));

    public void AddMetadataApiClient(string publicBasePath) =>
        serviceCollection.TryAddTransient<IMetadataApi>(sp => new MetadataApi(publicBasePath));

    public void AddMetadataApiClient(Uri publicBasePath) => AddMetadataApiClient(publicBasePath.ToString());
}
