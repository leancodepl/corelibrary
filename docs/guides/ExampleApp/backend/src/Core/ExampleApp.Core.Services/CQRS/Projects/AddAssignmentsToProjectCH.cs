using ExampleApp.Core.Contracts.Projects;
using ExampleApp.Core.Domain.Projects;
using ExampleApp.Core.Services.DataAccess;
using FluentValidation;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.DomainModels.DataAccess;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;

namespace ExampleApp.Core.Services.CQRS.Projects;

public class AddAssignmentsToProjectCV : ContextualValidator<AddAssignmentsToProject>
{
    public AddAssignmentsToProjectCV()
    {
        RuleFor(cmd => cmd.Assignments)
            .NotNull()
            .WithCode(AddAssignmentsToProject.ErrorCodes.AssignmentsCannotBeNull)
            .NotEmpty()
            .WithCode(AddAssignmentsToProject.ErrorCodes.AssignmentsCannotBeEmpty);

        RuleForEach(cmd => cmd.Assignments)
            .ChildRules(child => child.RuleFor(c => c).SetValidator(new AssignmentDTOValidator()));

        RuleForAsync(cmd => cmd, CheckProjectExistsAsync)
            .Equal(true)
            .WithCode(AddAssignmentsToProject.ErrorCodes.ProjectDoesNotExist)
            .WithMessage("A project with given Id does not exist.");
    }

    private Task<bool> CheckProjectExistsAsync(IValidationContext ctx, AddAssignmentsToProject command)
    {
        if (!SId<Project>.TryParse(command.ProjectId, out var projectId))
        {
            return Task.FromResult(false);
        }

        var appContext = ctx.AppContext<CoreContext>();
        var dbContext = ctx.GetService<CoreDbContext>();

        return dbContext.Projects.AnyAsync(p => p.Id == projectId, appContext.CancellationToken);
    }
}

public class AssignmentDTOValidator : AbstractValidator<AssignmentDTO>
{
    public AssignmentDTOValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithCode(AssignmentDTO.ErrorCodes.NameCannotBeEmpty)
            .MaximumLength(500)
            .WithCode(AssignmentDTO.ErrorCodes.NameTooLong);
    }
}

public class AddAssignmentsToProjectCH : ICommandHandler<CoreContext, AddAssignmentsToProject>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<AddAssignmentsToProjectCH>();

    private readonly IRepository<Project, SId<Project>> projects;

    public AddAssignmentsToProjectCH(IRepository<Project, SId<Project>> projects)
    {
        this.projects = projects;
    }

    public async Task ExecuteAsync(CoreContext context, AddAssignmentsToProject command)
    {
        var project = await projects.FindAndEnsureExistsAsync(
            SId<Project>.From(command.ProjectId),
            context.CancellationToken
        );
        project.AddAssignments(command.Assignments.Select(a => a.Name));

        projects.Update(project);

        logger.Information(
            "{AssignmentCount} assignments added to project {ProjectId}.",
            command.Assignments.Count,
            project.Id
        );
    }
}
