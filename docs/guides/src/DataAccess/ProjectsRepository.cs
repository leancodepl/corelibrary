public class ProjectsRepository<TContext> : EFRepository<Purchase, SId<Purchase>, TContext>
    where TContext : DbContext
{
    public ProjectsRepository(TContext dbContext)
        : base(dbContext) { }

    public override Task<Purchase?> FindAsync(SId<Purchase> id, CancellationToken cancellationToken = default)
    {
        return DbSet.AsTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken)!;
    }
}
