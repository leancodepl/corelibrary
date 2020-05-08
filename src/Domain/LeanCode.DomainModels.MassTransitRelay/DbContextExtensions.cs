using System;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.MassTransitRelay
{
    public static class DbContextExtensions
    {
        public static string GetFullTableName(this DbContext dbContext, Type entity)
        {
            var type = dbContext.Model.FindEntityType(entity);
            return $"[{type.GetSchema()}].[{type.GetTableName()}]";
        }
    }
}
