using System.Linq;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF;

public static class DbContextExtensions
{
    public static void SoftDeleteItems(this DbContext dbContext)
    {
        var softDeletables = dbContext
            .ChangeTracker.Entries()
            .Where(p => p.State == EntityState.Deleted && p.Entity is ISoftDeletable);

        foreach (var entry in softDeletables)
        {
            entry.State = EntityState.Modified;
            entry.CurrentValues[nameof(ISoftDeletable.IsDeleted)] = true;
        }
    }
}
