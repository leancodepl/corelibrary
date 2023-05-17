public class User : IAggregateRoot<SId<User>>
{
    public SId<User> Id { get; private init; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Task() { }

    public static Task Create(string name, string email)
    {
        return new Task
        {
            Id = SId<User>.New(),
            Name = name,
            Email = email,
        };
    }
}
