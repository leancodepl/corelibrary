using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public static class ModelBuilderExtensions
    {
        public static void EnableSoftDeleteOf<TSoftDeletable>(this ModelBuilder builder)
            where TSoftDeletable : class, ISoftDeletable
        {
            builder.Entity<TSoftDeletable>()
                .HasQueryFilter(e => e.IsDeleted == false);
        }
    }
}
