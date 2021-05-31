using System;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public static class DbContextExtensions
    {
        public static string GetFullTableName(this DbContext dbContext, Type entity)
        {
            var type = dbContext.Model.FindEntityType(entity);

            if (type is null)
            {
                throw new InvalidOperationException("Failed to find entity type.");
            }

            var schema = type.GetSchema();

            return string.IsNullOrEmpty(schema)
                ? $"[{type.GetTableName()}]"
                : $"[{schema}].[{type.GetTableName()}]";
        }
    }
}
