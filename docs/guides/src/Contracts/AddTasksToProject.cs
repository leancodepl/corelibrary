[AllowUnauthorized]
public class AddTasksToProject : ICommand
{
    public string ProjectId { get; set; }
    public List<TaskDTO> Tasks { get; set; }

    public static class ErrorCodes
    {
        public const int ProjectDoesNotExist = 1;
        public const int TasksCannotBeNull = 2;
        public const int TasksCannotBeEmpty = 3;
    }
}

public class TaskDTO
{
    public string Name { get; set; }

    public class ErrorCodes
    {
        public const int NameCannotBeEmpty = 101;
        public const int NameTooLong = 102;
    }
}
