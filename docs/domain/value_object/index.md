# Value object

In Domain-Driven Design, a value object is a concept used to model a descriptive aspect of the domain with no conceptual identity. In contrast to entities, which are defined by their identity, value objects are defined by their attributes. Essentially, a value object is a small, immutable object that represents a descriptive aspect of the domain.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.DomainModels | [![NuGet version (LeanCode.DomainModels)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.DomainModels/8.0.2260-preview/) | `IAggregateRoot`, `ValueObject` |
| LeanCode.DomainModels.Generators | [![NuGet version (LeanCode.DomainModels.Generators)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.Generators.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.DomainModels.Generators/8.0.2260-preview/) | Ids |

## Example

Let's add an address to `Employee` aggregate defined in [aggregate section](../aggregate/index.md#employee).

```csharp
public class Employee : IAggregateRoot<EmployeeId>
{
    // . . .
    public Address? Address { get; private set; }
    // . . .

    public void SetAddress(Address address)
    {
        Address = address;
    }
}

public record Address(string Street, string City) : ValueObject
{
    public string Street { get; private init; } = Street;
    public string City { get; private init; } = City;
}
```

Notice that `Address` class is defined as a record in C#. Records are a concise way to create immutable objects, making them suitable for value objects. `Address` is a value object as it's defined only by its properties, meaning that two addresses with same properties are treated as the same address.

## How to use records as value objects

* All properties must have `private init` setters.
* Always remember that C# uses default value comparator (in case of arrays and collections this is reference equality).
