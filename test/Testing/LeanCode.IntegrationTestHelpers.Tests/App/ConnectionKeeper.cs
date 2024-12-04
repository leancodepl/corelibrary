using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LeanCode.IntegrationTestHelpers.Tests.App;

public class ConnectionKeeper : IHostedService
{
    private readonly AsyncServiceScope scope;
    private readonly TestDbContext dbContext;

    public ConnectionKeeper(IServiceProvider serviceProvider)
    {
        scope = serviceProvider.CreateAsyncScope();

        dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
    }

    public Task StartAsync(CancellationToken cancellationToken) =>
        dbContext.Database.OpenConnectionAsync(cancellationToken);

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.CloseConnectionAsync();
        await scope.DisposeAsync();
    }
}
