public class Task : IIdentifiable<SId<Task>>
{
    public SId<Task> Id { get; private init; }
    public string Name { get; private set; }
    public TaskStatus Status { get; private set; }
    public SId<User>? AssignedUser { get; private set; }

    public Project ParentProject { get; private init; }

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Task() { }

    public static Task Create(Project parentProject, string name)
    {
        return new Task
        {
            Name = name,
            ParentProject = parentProject,
            Status = TaskStatus.NotStarted,
        };
    }

    public void Edit(string name)
    {
        Name = name;
    }

    public void AssignUser(SId<User> userId)
    {
        AssignedUser = userId;
    }

    public void UnassignUser()
    {
        AssignUser = null;
    }

    public void ChangeStatus(TaskStatus status)
    {
        Status = status;
    }

    public enum TaskStatus
    {
        NotStarted,
        InProgress,
        Finished,
    }
}
