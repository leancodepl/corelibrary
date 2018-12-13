using System;
using System.Reflection;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanCode.DomainModels.EF
{
    public static class ModelBuilderExtensions
    {
        private static readonly Type IOptimisticConcurrencyType = typeof(IOptimisticConcurrency);

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

        public static void IsOptimisticConcurrent<TEntity>(
            this EntityTypeBuilder<TEntity> cfg,
            bool useExplicitBackingFields = true)
            where TEntity : class, IOptimisticConcurrency
        {
            cfg.Property(e => e.RowVersion)
                .HasColumnName(nameof(IOptimisticConcurrency.RowVersion))
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            cfg.Property(e => e.DateModified)
                .HasColumnName(nameof(IOptimisticConcurrency.DateModified));

            if (useExplicitBackingFields)
            {
                cfg.Property(e => e.RowVersion)
                    .HasField(GetBackingFieldFor<TEntity>(nameof(IOptimisticConcurrency.RowVersion)));

                cfg.Property(e => e.DateModified)
                    .HasField(GetBackingFieldFor<TEntity>(nameof(IOptimisticConcurrency.DateModified)));
            }
        }

        public static void EnableOptimisticConcurrency<TEntity>(
            this ModelBuilder builder,
            bool useExplicitBackingFields = true)
            where TEntity : class, IOptimisticConcurrency
        {
            builder.Entity<TEntity>().IsOptimisticConcurrent(useExplicitBackingFields);
        }

        private static string GetBackingFieldFor<TEntity>(string fieldName)
        {
            return
                GetBackingFieldWhenAvailable<TEntity>($"<{IOptimisticConcurrencyType.FullName}.{fieldName}>k__BackingField") ??
                GetBackingFieldWhenAvailable<TEntity>($"<{fieldName}>k__BackingField") ??
                throw new InvalidOperationException($"Cannot find explicitly named backing field for IOptimisticConcurrency on type {typeof(TEntity).Name}.");
        }

        private static string GetBackingFieldWhenAvailable<TEntity>(string fieldName)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
            if (typeof(TEntity).GetField(fieldName, flags) is FieldInfo fi)
            {
                return fi.Name;
            }
            else
            {
                return null;
            }
        }
    }
}
