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
        Assert.Equal(a.I, b.I);
        Assert.Equal(a.J, b.J);
        Assert.Equal(a.K, b.K);
        Assert.Equal(a.L, b.L);
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
                configurationBuilder.Properties<StringId>().AreStringTypedId();
                configurationBuilder.Properties<PrefixedGuidId>().ArePrefixedTypedId();
                configurationBuilder.Properties<PrefixedStringId>().ArePrefixedTypedId();

                configurationBuilder.Properties<IntId?>().AreIntTypedId();
                configurationBuilder.Properties<LongId?>().AreLongTypedId();
                configurationBuilder.Properties<GuidId?>().AreGuidTypedId();
                configurationBuilder.Properties<StringId?>().AreStringTypedId();
                configurationBuilder.Properties<PrefixedGuidId?>().ArePrefixedTypedId();
                configurationBuilder.Properties<PrefixedStringId?>().ArePrefixedTypedId();
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
                    cfg.Property(e => e.D).IsStringTypedId();
                    cfg.Property(e => e.E).IsPrefixedTypedId();
                    cfg.Property(e => e.F).IsPrefixedTypedId();
                    cfg.Property(e => e.G).IsIntTypedId();
                    cfg.Property(e => e.H).IsLongTypedId();
                    cfg.Property(e => e.I).IsGuidTypedId();
                    cfg.Property(e => e.J).IsStringTypedId();
                    cfg.Property(e => e.K).IsPrefixedTypedId();
                    cfg.Property(e => e.L).IsPrefixedTypedId();
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
        public StringId D { get; set; }
        public PrefixedGuidId E { get; set; }
        public PrefixedStringId F { get; set; }

        public IntId? G { get; set; }
        public LongId? H { get; set; }
        public GuidId? I { get; set; }
        public StringId? J { get; set; }
        public PrefixedGuidId? K { get; set; }
        public PrefixedStringId? L { get; set; }

        public static Entity CreateFull()
        {
            return new Entity
            {
                A = new(1),
                B = new(2),
                C = new(Guid.NewGuid()),
                D = new("a"),
                E = new(Guid.NewGuid()),
                F = PrefixedStringId.FromValuePart("b"),
                G = new(3),
                H = new(4),
                I = new(Guid.NewGuid()),
                J = new("c"),
                K = new(Guid.NewGuid()),
                L = PrefixedStringId.FromValuePart("d"),
            };
        }

        public static Entity CreatePartial()
        {
            return new Entity
            {
                A = new(5),
                B = new(6),
                C = new(Guid.NewGuid()),
                D = new("e"),
                E = new(Guid.NewGuid()),
                F = PrefixedStringId.FromValuePart("f"),
            };
        }
    }
}
