using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeanCode.DomainModels.MassTransitRelay.Tests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2000", Justification = "Allowed in tests.")]
    public class DbContextExtensionsTests
    {
        [Fact]
        public void Full_table_name_is_correct_if_default_schema_is_present()
        {
            var tableName = new DefaultSchema().GetFullTableName(typeof(Entity));
            Assert.Equal("[test].[Entities]", tableName);
        }

        [Fact]
        public void Full_table_name_is_correct_if_explicit_schema_is_set()
        {
            var tableName = new ExplicitSchema().GetFullTableName(typeof(Entity));
            Assert.Equal("[test2].[Entities2]", tableName);
        }

        [Fact]
        public void Full_table_name_is_correct_if_no_schema_is_set()
        {
            var tableName = new NoSchema().GetFullTableName(typeof(Entity));
            Assert.Equal("[Entities]", tableName);
        }

        [Fact]
        public void Full_table_name_is_correct_if_schema_is_explicitly_unset()
        {
            var tableName = new ExplicitNoSchema().GetFullTableName(typeof(Entity));
            Assert.Equal("[Entities]", tableName);
        }

        private class Entity
        {
            public int Id { get; set; }
        }

        private abstract class BaseContext : DbContext
        {
            public DbSet<Entity> Entities { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Filename=:memory:");
            }
        }

        private class DefaultSchema : BaseContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.HasDefaultSchema("test");
                modelBuilder.Entity<Entity>().HasKey(e => e.Id);
            }
        }

        private class ExplicitSchema : BaseContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.HasDefaultSchema("test");
                modelBuilder.Entity<Entity>(cfg =>
                {
                    cfg.ToTable("Entities2", "test2");
                    cfg.HasKey(e => e.Id);
                });
            }
        }

        private class NoSchema : BaseContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Entity>().HasKey(e => e.Id);
            }
        }

        private class ExplicitNoSchema : BaseContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Entity>(cfg =>
                {
                    cfg.ToTable("Entities");
                    cfg.HasKey(e => e.Id);
                });
            }
        }
    }
}
