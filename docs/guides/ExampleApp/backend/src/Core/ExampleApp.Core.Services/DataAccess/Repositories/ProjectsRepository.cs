using ExampleApp.Core.Domain.Projects;
using LeanCode.DomainModels.EF;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace ExampleApp.Core.Services.DataAccess.Repositories;

public class ProjectsRepository : EFRepository<Project, SId<Project>, CoreDbContext>
{
    public ProjectsRepository(CoreDbContext dbContext)
        : base(dbContext) { }

    public override Task<Project?> FindAsync(SId<Project> id, CancellationToken cancellationToken = default)
    {
        return DbSet.AsTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken)!;
    }
}
