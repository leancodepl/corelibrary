using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public static class DbContextExtensions
    {
        public static string GetFullTableName(this DbContext dbContext, Type entity)
        {
            var entityType = dbContext.Model.FindEntityType(entity);

            if (entityType is null)
            {
                throw new InvalidOperationException("Failed to find entity type.");
            }

            var table = entityType.GetTableName();

            if (table is null)
            {
                throw new InvalidOperationException("Entity is not mapped to database table.");
            }

            var helper = dbContext.GetService<ISqlGenerationHelper>();

            return entityType.GetSchema() is string schema
                ? $"{helper.DelimitIdentifier(schema)}.{helper.DelimitIdentifier(table)}"
                : helper.DelimitIdentifier(table);
        }

        public static string GetColumnName(this DbContext dbContext, Type entity, string propertyName)
        {
            var entityType = dbContext.Model.FindEntityType(entity);

            if (entityType is null)
            {
                throw new InvalidOperationException("Failed to find entity type.");
            }

            var table = entityType.GetTableName();

            if (table is null)
            {
                throw new InvalidOperationException("Entity is not mapped to database table.");
            }

            var storeObject = StoreObjectIdentifier.Table(table, entityType.GetSchema());

            var property = entityType.FindProperty(propertyName);

            if (property is null)
            {
                throw new InvalidOperationException($"Property with name '{propertyName}' is not defined.");
            }

            var column = property.FindColumn(in storeObject);

            if (column is null)
            {
                throw new InvalidOperationException($"Property with name '{propertyName}' is not mapped to a column.");
            }

            return dbContext.GetService<ISqlGenerationHelper>().DelimitIdentifier(column.Name);
        }
    }
}
