public class Project : IAggregateRoot<SId<Project>>
{
    private readonly List<Task> tasks = new();

    public SId<Project> Id { get; private init; }
    public string Name { get; private set; }

    public IReadOnlyList<Task> Tasks => tasks;

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Project() { }

    public static Project Create(string name)
    {
        return new Project
        {
            Id = SId<Project>.New(),
            Name = name,
        };
    }

    public void EditTask(SId<Task> taskId, string name)
    {
        tasks.Single(t => t.Id == taskId).Edit(name);
    }

    public void AssignUserToTask(SId<Task> taskId, SId<User> userId)
    {
        tasks.Single(t => t.Id == taskId).AssignUser(userId);
        DomainEvents.Raise(new UserAssignedToTask(SId<Task> taskId, SId<User> userId));
    }

    public void UnassignUserFromTask(SId<Task> taskId)
    {
        tasks.Single(t => t.Id == taskId).UnassignUser(userId);
    }

    public void ChangeTaskStatus(SId<Task> taskId, TaskStatus status)
    {
        tasks.Single(t => t.Id == taskId).ChangeTaskStatus(status);
    }
}