public class User : IAggregateRoot<SId<User>>
{
    public SId<User> Id { get; private init; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private User() { }

    public static User Create(string name, string email)
    {
        return new User
        {
            Id = SId<User>.New(),
            Name = name,
            Email = email,
        };
    }
}
