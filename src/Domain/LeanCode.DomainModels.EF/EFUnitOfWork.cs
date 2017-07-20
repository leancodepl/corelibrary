using Microsoft.EntityFrameworkCore;
using LeanCode.DomainModels.DataAccess;
using System.Threading.Tasks;

namespace LeanCode.DomainModels.EF
{
    public class EFUnitOfWork : DbContext, IUnitOfWork
    {
        public EFUnitOfWork(DbContextOptions options)
            : base(options)
        { }

        public Task CommitAsync()
        {
            return base.SaveChangesAsync();
        }
    }
}
