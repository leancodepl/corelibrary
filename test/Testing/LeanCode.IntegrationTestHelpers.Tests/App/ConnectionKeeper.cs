using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace LeanCode.IntegrationTestHelpers.Tests.App;

public class ConnectionKeeper : IHostedService
{
    private readonly TestDbContext dbContext;

    public ConnectionKeeper(TestDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task StartAsync(CancellationToken cancellationToken) => dbContext.Database.OpenConnectionAsync(cancellationToken);
    public Task StopAsync(CancellationToken cancellationToken) => dbContext.Database.CloseConnectionAsync();
}
