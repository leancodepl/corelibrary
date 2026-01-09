using LeanCode.DomainModels.Ulids;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

public class TypedIdDatabaseIntegrationTests
{
    [Fact]
    public void Data_is_stored_and_restored_correctly()
    {
        Do(RegistrationMethod.Explicit);
        Do(RegistrationMethod.ConventionManual);
        Do(RegistrationMethod.ConventionAssemblyScan);

        static void Do(RegistrationMethod registrationMethod)
        {
            using var dbContext = TestDbContext.Create(registrationMethod);
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
        Assert.Equal(a.M, b.M);
        Assert.Equal(a.N, b.N);
    }

    private sealed class TestDbContext : DbContext
    {
        private readonly RegistrationMethod registrationMethod;

        public DbSet<Entity> Entities => Set<Entity>();

        public TestDbContext(RegistrationMethod registrationMethod, DbContextOptions<TestDbContext> options)
            : base(options)
        {
            this.registrationMethod = registrationMethod;
        }

        public static TestDbContext Create(RegistrationMethod registrationMethod)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase($"TestDb{Guid.NewGuid():N}")
                .Options;
            return new(registrationMethod, options);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            if (registrationMethod == RegistrationMethod.ConventionManual)
            {
                configurationBuilder.Properties<IntId>().AreIntTypedId();
                configurationBuilder.Properties<LongId>().AreLongTypedId();
                configurationBuilder.Properties<GuidId>().AreGuidTypedId();
                configurationBuilder.Properties<StringId>().AreStringTypedId();
                configurationBuilder.Properties<PrefixedGuidId>().ArePrefixedTypedId();
                configurationBuilder.Properties<PrefixedUlidId>().ArePrefixedTypedId();
                configurationBuilder.Properties<PrefixedStringId>().ArePrefixedTypedId();

                configurationBuilder.Properties<IntId?>().AreIntTypedId();
                configurationBuilder.Properties<LongId?>().AreLongTypedId();
                configurationBuilder.Properties<GuidId?>().AreGuidTypedId();
                configurationBuilder.Properties<StringId?>().AreStringTypedId();
                configurationBuilder.Properties<PrefixedGuidId?>().ArePrefixedTypedId();
                configurationBuilder.Properties<PrefixedUlidId?>().ArePrefixedTypedId();
                configurationBuilder.Properties<PrefixedStringId?>().ArePrefixedTypedId();
            }

            if (registrationMethod == RegistrationMethod.ConventionAssemblyScan)
            {
                configurationBuilder.ConfigureTypedIdsConventions(typeof(IntId).Assembly);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>(cfg =>
            {
                if (registrationMethod == RegistrationMethod.Explicit)
                {
                    cfg.Property(e => e.A).IsIntTypedId();
                    cfg.Property(e => e.B).IsLongTypedId();
                    cfg.Property(e => e.C).IsGuidTypedId();
                    cfg.Property(e => e.D).IsStringTypedId();
                    cfg.Property(e => e.E).IsPrefixedTypedId();
                    cfg.Property(e => e.F).IsPrefixedTypedId();
                    cfg.Property(e => e.G).IsPrefixedTypedId();
                    cfg.Property(e => e.H).IsIntTypedId();
                    cfg.Property(e => e.I).IsLongTypedId();
                    cfg.Property(e => e.J).IsGuidTypedId();
                    cfg.Property(e => e.K).IsStringTypedId();
                    cfg.Property(e => e.L).IsPrefixedTypedId();
                    cfg.Property(e => e.M).IsPrefixedTypedId();
                    cfg.Property(e => e.N).IsPrefixedTypedId();
                }

                cfg.HasKey(e => e.A);
            });
        }
    }

    private enum RegistrationMethod
    {
        Explicit,
        ConventionManual,
        ConventionAssemblyScan,
    }

    private sealed record Entity
    {
        public IntId A { get; set; }
        public LongId B { get; set; }
        public GuidId C { get; set; }
        public StringId D { get; set; }
        public PrefixedGuidId E { get; set; }
        public PrefixedUlidId F { get; set; }
        public PrefixedStringId G { get; set; }

        public IntId? H { get; set; }
        public LongId? I { get; set; }
        public GuidId? J { get; set; }
        public StringId? K { get; set; }
        public PrefixedGuidId? L { get; set; }
        public PrefixedUlidId? M { get; set; }
        public PrefixedStringId? N { get; set; }

        public static Entity CreateFull()
        {
            return new()
            {
                A = new(1),
                B = new(2),
                C = new(Guid.NewGuid()),
                D = new("a"),
                E = new(Guid.NewGuid()),
                F = new(Ulid.NewUlid()),
                G = PrefixedStringId.FromValuePart("b"),
                H = new(3),
                I = new(4),
                J = new(Guid.NewGuid()),
                K = new("c"),
                L = new(Guid.NewGuid()),
                M = new(Ulid.NewUlid()),
                N = PrefixedStringId.FromValuePart("d"),
            };
        }

        public static Entity CreatePartial()
        {
            return new()
            {
                A = new(5),
                B = new(6),
                C = new(Guid.NewGuid()),
                D = new("e"),
                E = new(Guid.NewGuid()),
                F = new(Ulid.NewUlid()),
                G = PrefixedStringId.FromValuePart("f"),
            };
        }
    }
}
