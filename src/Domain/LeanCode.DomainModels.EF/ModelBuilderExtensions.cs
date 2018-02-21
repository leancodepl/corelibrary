using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanCode.DomainModels.EF
{
    public static class ModelBuilderExtensions
    {
        public static void EnableSoftDeleteOf<TSoftDeletable>(this ModelBuilder builder)
            where TSoftDeletable : class, ISoftDeletable
        {
            builder.Entity<TSoftDeletable>().IsSoftDeletable();
        }

        public static void IsSoftDeletable<TEntity>(this EntityTypeBuilder<TEntity> cfg)
            where TEntity : class, ISoftDeletable
        {
            cfg.HasQueryFilter(e => e.IsDeleted == false);
        }

        public static void IsOptimisticConcurrent<TEntity>(this EntityTypeBuilder<TEntity> cfg)
            where TEntity : class, IOptimisticConcurrency
        {
            cfg.Property(e => e.RowVersion)
                .HasColumnName(nameof(IOptimisticConcurrency.RowVersion))
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            cfg.Property(e => e.DateModified)
                .HasColumnName(nameof(IOptimisticConcurrency.DateModified));
        }

        public static void EnableOptimisticConcurrency<TEntity>(this ModelBuilder builder)
            where TEntity : class, IOptimisticConcurrency
        {
            builder.Entity<TEntity>().IsOptimisticConcurrent();
        }
    }
}
