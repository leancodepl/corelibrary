#nullable enable
using FluentAssertions;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

public class EFRepositoryTrackingCompositeTests
{
    private readonly InMemoryDatabaseRoot dbRoot = new();
    private readonly Guid dbId = Guid.NewGuid();

    [Fact]
    public void Does_not_return_tracked_entity_if_it_is_not_there()
    {
        var (_, repository) = Prepare();

        repository.FindTracked((1, 10)).Should().BeNull();
    }

    [Fact]
    public void Returns_tracked_entity_if_it_is_not_in_the_database_but_has_been_added_to_the_tracker()
    {
        var (_, repository) = Prepare();
        var entity = new Entity(1, 2);

        repository.Add(entity);

        repository.FindTracked(entity.Id).Should().Be(entity);
    }

    [Fact]
    public void Does_not_return_entity_if_it_has_been_tracked_in_different_context()
    {
        var (_, repository1) = Prepare();
        var (_, repository2) = Prepare();
        var entity = new Entity(1, 2);

        repository1.Add(entity);

        repository2.FindTracked(entity.Id).Should().BeNull();
    }

    [Fact]
    public void Does_not_return_entity_if_it_is_in_database_but_has_not_been_tracked_yet()
    {
        var entity = new Entity(1, 2);
        SaveEntity(entity);

        var (_, repository) = Prepare();
        repository.FindTracked(entity.Id).Should().BeNull();
    }

    [Fact]
    public void Returns_entity_if_it_was_tracked_already()
    {
        var entity = new Entity(1, 2);
        SaveEntity(entity);

        var (ctx, repository) = Prepare();
        ctx.Entities.Find(entity.Id1, entity.Id2); // Load the entity

        repository.FindTracked(entity.Id).Should().BeEquivalentTo(entity);
        repository.FindTracked(entity.Id).Should().NotBeSameAs(entity);
    }

    [Fact]
    public void If_the_entity_is_tracked_but_gets_deleted_from_the_underlying_database_It_is_still_returned()
    {
        var entity = new Entity(1, 2);
        SaveEntity(entity);

        var (ctx, repository) = Prepare();
        ctx.Entities.Find(entity.Id1, entity.Id2); // Load the entity

        DeleteEntity(entity);

        repository.FindTracked(entity.Id).Should().BeEquivalentTo(entity);
        repository.FindTracked(entity.Id).Should().NotBeSameAs(entity);
    }

    [Fact]
    public async Task Loading_the_entity_tracks_it_and_then_returns_tracked_entity_in_the_consecutive_calls()
    {
        var entity = new Entity(1, 2);
        SaveEntity(entity);

        var (_, repository) = Prepare();

        var existing1 = await repository.FindAsync(entity.Id);
        var existing2 = await repository.FindAsync(entity.Id);

        existing1.Should().BeEquivalentTo(entity);
        existing2.Should().Be(existing1);
    }

    [Fact]
    public async Task If_the_entity_is_loaded_It_stays_tracked_even_if_it_gets_deleted_from_the_database()
    {
        var entity = new Entity(1, 2);
        SaveEntity(entity);

        var (_, repository) = Prepare();

        var existing1 = await repository.FindAsync(entity.Id);
        DeleteEntity(entity);

        var existing2 = await repository.FindAsync(entity.Id);

        existing1.Should().BeEquivalentTo(entity);
        existing2.Should().Be(existing1);
    }

    [Fact]
    public void If_the_entity_is_loaded_from_database_in_different_context_It_is_not_tracked_in_another()
    {
        var entity = new Entity(1, 2);
        SaveEntity(entity);

        var (ctx, _) = Prepare();
        ctx.Entities.Find(entity.Id1, entity.Id2); // Load the entity

        var (_, anotherRepository) = Prepare();
        anotherRepository.FindTracked(entity.Id).Should().BeNull();
    }

    private void SaveEntity(Entity entity)
    {
        var (ctx1, repository) = Prepare();
        repository.Add(entity);
        ctx1.SaveChanges();
    }

    private void DeleteEntity(Entity entity)
    {
        var (ctx1, _) = Prepare();
        var existing = ctx1.Entities.First(e => e.Id1 == entity.Id1 && e.Id2 == entity.Id2);
        ctx1.Entities.Remove(existing);
        ctx1.SaveChanges();
    }

    private (TestDbContext, EntityRepository) Prepare()
    {
        var context = TestDbContext.Create(dbRoot, dbId);
        context.Database.EnsureCreated();
        return (context, new(context));
    }

    private sealed class TestDbContext : DbContext
    {
        public DbSet<Entity> Entities => Set<Entity>();

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options) { }

        public static TestDbContext Create(InMemoryDatabaseRoot root, Guid dbId)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase($"TestDb{dbId:N}", root)
                .Options;
            return new(options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>(cfg =>
            {
                cfg.HasKey(e => new { e.Id1, e.Id2 });
            });
        }
    }

    private sealed class EntityRepository : CachingEFRepository<Entity, (int, int), TestDbContext>
    {
        public EntityRepository(TestDbContext dbContext)
            : base(dbContext) { }

        public new Entity? FindTracked((int, int) id)
        {
            return base.FindTracked(id.Item1, id.Item2);
        }

        public override Task<Entity?> FindAsync((int, int) id, CancellationToken cancellationToken = default) =>
            FindTrackedOrLoadNewAsync(
                    new object[] { id.Item1, id.Item2 },
                    s => s.FirstOrDefaultAsync(e => e.Id1 == id.Item1 && e.Id2 == id.Item2, cancellationToken)
                )
                .AsTask();
    }

    private sealed record Entity(int Id1, int Id2) : IAggregateRootWithoutOptimisticConcurrency<(int, int)>
    {
        public (int, int) Id => (Id1, Id2);
    }
}
