# Aggregates

Aggregates are part of DDD. To create one you have to create a class inheriting [`IAggregateRoot`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs) or [`IAggregateRootWithoutOptimisticConcurrency`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs).

## [`IAggregateRoot`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs)

[`IAggregateRoot`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs) requires you to specify Id type.

Consider the following aggregate.

```csharp
[TypedId(TypedIdFormat.RawInt)]
public readonly partial record struct UserId;

public class User : IAggregateRoot<UserId>
{
    public UserId Id { get; private init; }
    public string Name { get; private init; } = null!;

    DateTime IOptimisticConcurrency.DateModified { get; set; }

    private User()
    { }

    public User(UserId id, string name)
    {
        Id = id;
        Name = name;
    }
}
```

### Parameterless constructor

Parameterless constructor is required by EntityFramework. It is private because it should not be used in other scenarios, especially application code.

### Id

[`IAggregateRoot`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs) requires you to specify identity type. Every aggregate has an `Id` property of the specified type.

### `IOptimisticConcurrency.DateModified`

`DateModified` is optimistic concurrency token managed by application code. It is managed by [`EFRepository`](../../src/Domain/LeanCode.DomainModels.EF/EFRepository.cs) and you shouldn't do it by yourself. It is written this way, instead of `public DateTime DateModified { get; set; }`, to make it not accessible outside [`EFRepository`](../../src/Domain/LeanCode.DomainModels.EF/EFRepository.cs).

## [`IAggregateRootWithoutOptimisticConcurrency`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs)

If you don't want to use optimistic concurrency for your aggregate, you can use [`IAggregateRootWithoutOptimisticConcurrency`](../../src/Domain/LeanCode.DomainModels/Model/IAggregateRoot.cs) instead.

Consider aggregate from previous example, but without optimistic concurrency.

```csharp
public class User : IAggregateRootWithoutOptimisticConcurrency<UserId>
{
    public UserId Id { get; private init; }
    public string Name { get; private init; } = null!;

    private User()
    { }

    public User(UserId id, string name)
    {
        Id = id;
        Name = name;
    }
}
```

## More

If you want to read more about writing aggregates, you can do that [here](../guides/01_creating_an_aggregate.md).
