# Aggregates

Aggregates are part of DDD. To create one you have to create entity class inheriting [`IAggregateRoot`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs) or [`IAggregateRootWithoutOptimisticConcurrency`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs).

## [`IAggregateRootWithoutOptimisticConcurrency`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs)

In order to create aggregate root you should create class inheriting it. [`IAggregateRootWithoutOptimisticConcurrency`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs) requires you to give Id type.

Consider the following aggregate.

```csharp
public class User : IAggregateRootWithoutOptimisticConcurrency<Id<User>>
{
    public Id<User> Id { get; private init; }
    public string Name { get; private init; } = null!;

    private User()
    { }

    public User(Id<User> id, string name)
    {
        Id = id;
        Name = name;
    }
}
```

### Parameterless constructor

Parameterless constructor is required by entity framework. It is private to not be accessible outside.

### Id

[`IAggregateRootWithoutOptimisticConcurrency`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs) requires you to specify identity type. Every aggregate has an `Id` property of the specified type.

## [`IAggregateRoot`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs)

[`IAggregateRoot`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs) extends [`IAggregateRootWithoutOptimisticConcurrency`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs) by adding `DateTime` optimistic concurrency token.

Consider aggregate from previous example, but with optimistic concurrency.

```csharp
public class User : IAggregateRoot<Id<User>>
{
    public Id<User> Id { get; private init; }
    public string Name { get; private init; } = null!;

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private User()
    { }

    public User(Id<User> id, string name)
    {
        Id = id;
        Name = name;
    }
}
```

### `IOptimisticConcurrency.DateModified`

`DateModified` is optimistic concurrency token managed by application code. It is managed by [`EFRepository`](../../src/Domain/LeanCode.DomainModels.EF/EFRepository.cs) and you shouldn't do it by yourself. It is written this way, instead of `public DateTime DateModified { get; set; }`, to make it not accessible outside [`EFRepository`](../../src/Domain/LeanCode.DomainModels.EF/EFRepository.cs).
