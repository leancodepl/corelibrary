using Microsoft.EntityFrameworkCore;
using LeanCode.DomainModels.DataAccess;

namespace LeanCode.DomainModels.EF
{
    public class EFUnitOfWork : DbContext, IUnitOfWork
    {
        public EFUnitOfWork(DbContextOptions options)
            : base(options)
        { }

        public void Commit()
        {
            base.SaveChanges();
        }
    }
}
