[AllowUnauthorized]
public class CreateProject : ICommand
{
    public string Name { get; set; }

    public static class ErrorCodes
    {
        public const int NameCannotBeEmpty = 1;
        public const int NameTooLong = 2;
    }
}
