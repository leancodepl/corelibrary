# Id

[Aggregate] requires you to specify identity type. Every aggregate has an `Id` property of the specified type. CoreLib supports three different flavors of IDs:

- Primitive types like `Guid`, `int`, etc.
- Generic type wrappers.
- Source generated IDs.

From CoreLib v8, Source Generated IDs are the default one, with primitive types being fallbacks if source generated one cannot be used.

## Packages

| Package | Link | Application in section |
| --- | ----------- | ----------- |
| LeanCode.DomainModels | [![NuGet version (LeanCode.DomainModels)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.DomainModels) | `IAggregateRoot` |
| LeanCode.DomainModels.Generators | [![NuGet version (LeanCode.DomainModels.Generators)](https://img.shields.io/nuget/vpre/LeanCode.DomainModels.Generators.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/LeanCode.DomainModels.Generators) | Ids |

## Source generated IDs

Source generated IDs leverage [Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) to generate fully functional ID structs that can work as IDs for entities. They are specialized for a particular entity type, but don't use the generic mechanisms of the language. They support the same feature set as aforementioned IDs.

### How to use source-generated IDs

To use the source-generated IDs, you first need to reference the source generator that does the heavy lifting:

```xml
<PackageReference Include="LeanCode.DomainModels.Generators" Version="(version)" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

Then, you need to add a partial struct record that will be filled up by the compiler when building the project, and decorate it with [TypedIdAttribute]:

```cs
[TypedId(TypedIdFormat.RawInt)]
public readonly partial record struct EmployeeId;
```

Then, you can use it as an aggregate id and across the project.

```cs
public class Employee : IAggregateRoot<EmployeeId>
{
    public EmployeeId Id { get; }

    // . . .
}
```

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

- `TypedIdFormat` - this configures the underlying type and the format of the ID. You need to specify it as a first parameter to the `TypedIdAttribute`. Possibilities:
    - `RawInt` - uses `int` as the underlying type; works as a wrapper over `int`; does not support generating new IDs at runtime by default.
    - `RawLong` - uses `long` as the underlying type; works as a wrapper over `long`; does not support generating new IDs at runtime by default.
    - `RawGuid` - uses `Guid` as the underlying type; works as a wrapper over `Guid`; can generate new ID at runtime using `Guid.NewGuid`.
    - `RawString` - uses `string` as the underlying type; works as a wrapper over arbitrary strings; does not support generating new IDs at runtime.
    - `PrefixedGuid` - uses `string` as the underlying type; it is represented as a `(prefix)_(guid)` string that can be generated at runtime; by default `(prefix)` is a lowercase class name with `Id` suffix removed.
    - `PrefixedUlid` - uses `string` as the underlying type; it is represented as a `(prefix)_(ulid)` string that can be generated at runtime; by default `(prefix)` is a lowercase class name with `Id` suffix removed.
    - `PrefixedString` - uses `string` as the underlying type; it is represented as a `(prefix)_(value)` string where `(value)` is an arbitrary string; does not support generating new IDs at runtime.
- `CustomPrefix` - for `Prefixed*` formats, you can configure what prefix it uses (if you e.g. want to use a shorter one).
- `SkipRandomGenerator` - setting this to `true` will skip generating `New` factory method (for formats that support generation).
- `MaxValueLength` - optional maximum length constraint for the value part. For `RawString`, this is the entire string length. For `PrefixedString`, this excludes the prefix and separator. When set, validation is performed in `Parse`/`IsValid` methods. Consider SQL Server's 900-byte key limit (~450 nvarchar chars) when choosing this value.

Examples:

```cs
[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "employee")]
public readonly partial record struct VeryLongEmployeeId;

// The `VeryLongEmployeeId` will have format `employee_(guid)`, with `New` using `Guid.NewGuid` as random source.
```

```cs
[TypedId(TypedIdFormat.RawString, MaxValueLength = 100)]
public readonly partial record struct ExternalId;

// The `ExternalId` wraps any string up to 100 characters. Exposes `MaxLength` static property.
```

```cs
[TypedId(TypedIdFormat.PrefixedString, CustomPrefix = "ext", MaxValueLength = 50)]
public readonly partial record struct ExternalRefId;

// The `ExternalRefId` has format `ext_(value)` where value can be up to 50 characters.
// Exposes `MaxValueLength` (50) and `MaxRawLength` (54 = 3 + 1 + 50) static properties.
```

## Generic type wrappers

The domain part of the library supports a set of generic IDs:

- [Id&lt;T&gt;]
- [IId&lt;T&gt;]
- [LId&lt;T&gt;]
- [SId&lt;T&gt;]

All the types give you type safety when passing the IDs, without introducing penalty (they basically work as `newtype`s). Unfortunately, they require an entity to be defined beforehand - it works as a generic parameter. This means you can't use it without a corresponding entity type. This poses a problem if you want to use the ID outside the parent domain. It is also quite hard to use - you need to know the exact ID format before you reference it (you need to choose between the four types when you just want to reference other entity).

[Id&lt;T&gt;]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/Domain/LeanCode.DomainModels/Model/Id.cs
[IId&lt;T&gt;]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/Domain/LeanCode.DomainModels/Model/Id.cs
[LId&lt;T&gt;]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/Domain/LeanCode.DomainModels/Model/Id.cs
[SId&lt;T&gt;]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/Domain/LeanCode.DomainModels/Model/Id.cs
[TypedIdAttribute]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/Domain/LeanCode.DomainModels/Ids/TypedIdAttribute.cs
[Aggregate]: ../aggregate/index.md
