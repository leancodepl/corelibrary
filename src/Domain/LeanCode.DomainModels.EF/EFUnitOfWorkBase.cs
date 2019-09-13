using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.DomainModels.EF
{
    public abstract class EFUnitOfWorkBase<TContext> : IUnitOfWork
        where TContext : notnull, DbContext
    {
        private readonly TContext dbContext;

        public EFUnitOfWorkBase(TContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public virtual Task CommitAsync()
        {
            SoftDeleteItems();
            return dbContext.SaveChangesAsync();
        }

        private void SoftDeleteItems()
        {
            var softDeletables = dbContext.ChangeTracker.Entries()
                .Where(p => p.State == EntityState.Deleted && p.Entity is ISoftDeletable);

            foreach (var entry in softDeletables)
            {
                entry.State = EntityState.Modified;
                entry.CurrentValues[nameof(ISoftDeletable.IsDeleted)] = true;
            }
        }
    }
}
