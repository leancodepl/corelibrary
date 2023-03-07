using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

public class TypedIdDatabaseIntegrationTests
{
    [Fact]
    public void Data_is_stored_and_restored_correctly()
    {
        Do(false);
        Do(true);

        static void Do(bool useConventions)
        {
            using var dbContext = TestDbContext.Create(useConventions);
            var a = Entity.CreateFull();
            var b = Entity.CreatePartial();

            dbContext.Database.EnsureCreated();

            dbContext.Entities.Add(a);
            dbContext.Entities.Add(b);
            dbContext.SaveChanges();

            dbContext.ChangeTracker.Clear();

            var entities = dbContext.Entities.ToList();
            var a2 = Assert.Single(entities, e => e.A == a.A);
            var b2 = Assert.Single(entities, e => e.A == b.A);
            AssertEqual(a, a2);
            AssertEqual(b, b2);
        }
    }

    private static void AssertEqual(Entity a, Entity b)
    {
        Assert.Equal(a.A, b.A);
        Assert.Equal(a.B, b.B);
        Assert.Equal(a.C, b.C);
        Assert.Equal(a.D, b.D);
        Assert.Equal(a.E, b.E);
        Assert.Equal(a.F, b.F);
        Assert.Equal(a.G, b.G);
        Assert.Equal(a.H, b.H);
    }

    private sealed class TestDbContext : DbContext
    {
        private readonly bool useConventions;

        public DbSet<Entity> Entities => Set<Entity>();

        public TestDbContext(bool useConventions, DbContextOptions<TestDbContext> options)
            : base(options)
        {
            this.useConventions = useConventions;
        }

        public static TestDbContext Create(bool useConventions)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase($"TestDb{Guid.NewGuid():N}")
                .Options;
            return new(useConventions, options);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            if (useConventions)
            {
                configurationBuilder.Properties<IntId>().AreIntTypedId();
                configurationBuilder.Properties<LongId>().AreLongTypedId();
                configurationBuilder.Properties<GuidId>().AreGuidTypedId();
                configurationBuilder.Properties<PrefixedGuidId>().ArePrefixedTypedId();

                configurationBuilder.Properties<IntId?>().AreIntTypedId();
                configurationBuilder.Properties<LongId?>().AreLongTypedId();
                configurationBuilder.Properties<GuidId?>().AreGuidTypedId();
                configurationBuilder.Properties<PrefixedGuidId?>().ArePrefixedTypedId();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>(cfg =>
            {
                if (!useConventions)
                {
                    cfg.Property(e => e.A).IsIntTypedId();
                    cfg.Property(e => e.B).IsLongTypedId();
                    cfg.Property(e => e.C).IsGuidTypedId();
                    cfg.Property(e => e.D).IsPrefixedTypedId();
                    cfg.Property(e => e.E).IsIntTypedId();
                    cfg.Property(e => e.F).IsLongTypedId();
                    cfg.Property(e => e.G).IsGuidTypedId();
                    cfg.Property(e => e.H).IsPrefixedTypedId();
                }

                cfg.HasKey(e => e.A);
            });
        }
    }

    private sealed record Entity
    {
        public IntId A { get; set; }
        public LongId B { get; set; }
        public GuidId C { get; set; }
        public PrefixedGuidId D { get; set; }

        public IntId? E { get; set; }
        public LongId? F { get; set; }
        public GuidId? G { get; set; }
        public PrefixedGuidId? H { get; set; }

        public static Entity CreateFull()
        {
            return new Entity
            {
                A = new(1),
                B = new(2),
                C = new(Guid.NewGuid()),
                D = new(Guid.NewGuid()),
                E = new(3),
                F = new(4),
                G = new(Guid.NewGuid()),
                H = new(Guid.NewGuid()),
            };
        }

        public static Entity CreatePartial()
        {
            return new Entity
            {
                A = new(5),
                B = new(6),
                C = new(Guid.NewGuid()),
                D = new(Guid.NewGuid()),
            };
        }
    }
}
