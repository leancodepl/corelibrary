using FluentAssertions;
using LeanCode.DomainModels.EF;
using LeanCode.IntegrationTests.App;
using LeanCode.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.IntegrationTests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1001", Justification = "Disposed with `IAsyncLifetime`.")]
public class EFRepositoryTests : IAsyncLifetime
{
    private readonly TestApp app = new();

    [IntegrationFact]
    public async Task Default_implementation_of_EFRepository_works()
    {
        var entity = new Entity { Id = EntityId.New(), Value = "test value" };

        await EnsureEntityDoesNotExistAsync(entity);
        await AddEntityAsync(entity);
        await EnsureEntityIsFoundAsync(entity);
    }

    private async Task EnsureEntityIsFoundAsync(Entity entity)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var repository = new EntityRepository(dbContext);

        var foundEntity = await repository.FindAsync(entity.Id);
        foundEntity.Should().BeEquivalentTo(entity);
    }

    private async Task AddEntityAsync(Entity entity)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var repository = new EntityRepository(dbContext);
        repository.Add(entity);
        await dbContext.SaveChangesAsync();
    }

    private async Task EnsureEntityDoesNotExistAsync(Entity entity)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var repository = new EntityRepository(dbContext);

        var foundEntity = await repository.FindAsync(entity.Id);
        foundEntity.Should().BeNull();
    }

    public async ValueTask InitializeAsync() => await app.InitializeAsync();

    public ValueTask DisposeAsync() => app.DisposeAsync();

    private sealed class EntityRepository : EFRepository<Entity, EntityId, TestDbContext>
    {
        public EntityRepository(TestDbContext dbContext)
            : base(dbContext) { }
    }
}
