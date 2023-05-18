public class AddTasksToProjectCV : ContextualValidator<AddTasksToProject>
{
    public AddTasksToProjectCV()
    {
        RuleFor(cmd => cmd.Tasks)
            .NotNull()
            .WithCode(AddTasksToProject.ErrorCodes.TasksCannotBeNull)
            .NotEmpty()
            .WithCode(AddTasksToProject.ErrorCodes.TasksCannotBeEmpty);

        RuleForEach(cmd => cmd.Tasks).ChildRules(child => child.RuleFor(c => c).SetValidator(new TaskDTOValidator()));

        RuleForAsync(cmd => cmd, CheckProjectExistsAsync)
            .Equal(true)
            .WithCode(AddContactToSite.ErrorCodes.ProjectDoesNotExist)
            .WithMessage("A project with given Id does not exist.");
    }

    private Task<bool> CheckProjectExistsAsync(IValidationContext ctx, AddTasksToProject command)
    {
        if (!SId<Project>.TryParse(command.ProjectId, out var projectId))
        {
            return false;
        }

        var appContext = ctx.AppContext<CoreContext>();
        var dbContext = ctx.GetService<CoreDbContext>();

        return dbContext.Projects.AnyAsync(p => p.Id == projectId, appContext.CancellationToken);
    }
}

public class TaskDTOValidator : AbstractValidator<TaskDTO>
{
    public TaskDTOValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithCode(TaskDTO.ErrorCodes.NameCannotBeEmpty)
            .MaximumLength(500)
            .WithCode(TaskDTO.ErrorCodes.NameTooLong);
    }
}

public class AddTasksToProjectCH : ICommandHandler<CoreContext, AddTasksToProject>
{
    private readonly Serilog.ILogger logger = Serilog.Log.ForContext<AddTasksToProjectCH>();

    private readonly IRepository<Project, SId<Project>> projects;

    public AddTasksToProjectCH(IRepository<Project, SId<Project>> projects)
    {
        this.projects = projects;
    }

    public async Task ExecuteAsync(CoreContext context, AddTasksToProject command)
    {
        var project = await projects.FindAndEnsureExistsAsync(command.ProjectId, context.CancellationToken);
        project.AddTasks(command.Tasks.Select(t => Task.Create(t.Name)));

        projects.Update(project);

        logger.Information("{TaskCount} tasks added to project {ProjectId}.", command.Tasks.Count, project.Id);
    }
}
