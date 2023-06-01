using LeanCode.DomainModels.Model;

namespace ExampleApp.Core.Domain.Projects;

public class Employee : IAggregateRoot<SId<Employee>>
{
    public SId<Employee> Id { get; private init; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private Employee() { }

    public static Employee Create(string name, string email)
    {
        return new Employee
        {
            Id = SId<Employee>.New(),
            Name = name,
            Email = email,
        };
    }
}
