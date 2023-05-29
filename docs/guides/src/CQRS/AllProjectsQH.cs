public class AllProjectsQH : IQueryHandler<CoreContext, AllProjects, List<ProjectDTO>>
{
    private readonly CoreDbContext dbContext;

    public AllProjectsQH(CoreDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<ProjectDTO>> ExecuteAsync(CoreContext context, AllProjects query)
    {
        var q = dbContext.Projects.Select(p => new ProjectDTO { Id = p.Id, Name = p.Name, });

        q = query.SortByNameDescending ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name);

        return q.ToListAsync(context.CancellationToken);
    }
}
