# IDs

The domain part of the library supports a set of generic IDs:

1. [`Id<T>`](../../src/Domain/LeanCode.DomainModels/Model/Id.cs),
2. [`IId<T>`](../../src/Domain/LeanCode.DomainModels/Model/Id.cs),
3. [`LId<T>`](../../src/Domain/LeanCode.DomainModels/Model/Id.cs),
4. [`SId<T>`](../../src/Domain/LeanCode.DomainModels/Model/SId.cs).

All the types give you type safety when passing the IDs, without introducing penalty (they basically work as `newtype`s). Unfortunately, they require an entity to be defined beforehand - it works as a generic parameter. This means you can't use it without a corresponding entity type. This poses a problem if you want to use the ID outside the parent domain. It is also quite hard to use - you need to know the exact ID format before you reference it (you need to choose between the four types when you just want to reference other entity).

Since v7.0 we introduced another ID type - source generated IDs - that solve the problem.

## Source generated IDs

Source generated IDs leverage [Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) to generate fully functional ID structs that can work as IDs for entities. They are specialized for a particular entity type, but don't use the generic mechanisms of the language. They support the same feature set as aforementioned IDs.

### How to use

To use the source-generated IDs, you first need to reference the source generator that does the heavy lifting:

```xml
<PackageReference Include="LeanCode.DomainModels.Generators" Version="(version)" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

Then, you need to add a partial struct record that will be filled up by the compiler when building the project, and decorate it with [`TypedIdAttribute`](../../src/Domain/LeanCode.DomainModels/Ids/TypedIdAttribute.cs):

```cs
[TypedId(TypedIdFormat.RawInt)]
public readonly partial record struct UserId;
```

Then, you can use it as an aggregate ID:

```cs
public class User : IAggregateRoot<UserId>
{
    public UserId Id { get; }
    DateTime IOptimisticConcurrency.DateModified { get; set; }
}
```

And use it across the project.

### API

The generated ID supports the following operations:

```cs
public readonly partial record struct ID
{
    public static readonly TestIntId Empty;

    public int Value { get; }
    public bool IsEmpty { get; }

    public static ID Parse(int? v);
    public static ID? ParseNullable(int? id);
    public static bool TryParse([NotNullWhen(true)] int? v, out ID id);
    public static bool IsValid([NotNullWhen(true)] int? v);

    public static ID New(); // Only if generation is possible
}
```

### Configuration

The format of the ID can be configured using:

1. `TypedIdFormat` - this configures the underlying type and the format of the ID. You need to specify it as a first parameter to the `TypedIdAttribute`. Possibilities:
   a) `RawInt` - uses `int` as the underlying type; works as a wrapper over `int`; does not support generating new IDs at runtime by default;
   b) `RawLong` - uses `long` as the underlying type; works as a wrapper over `long`; does not support generating new IDs at runtime by default;
   c) `RawGuid` - uses `Guid` as the underlying type; works as a wrapper over `Guid`; can generate new ID at runtime using `Guid.NewGuid`;
   d) `PrefixedGuid` - uses `string` as the underlying type; it is represented as a `(prefix)_(guid)` string that can be generated at runtime; by default `(prefix)` is a lowercase class name with `id` at the end removed;
2. `CustomPrefix` - for `Prefixed*` formats, you can configure what prefix it uses (if you e.g. want to use a shorter one),
3. `SkipRandomGenerator` - setting this to `true` will skip generating `New` factory method (for `Prefixed` types only).

Example:

```cs
[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "user")]
public readonly partial record struct VeryLongUserId;

// The `VeryLongUserId` will have format `user_(guid)`, with `New` using `Guid.NewGuid` as random source.
```
