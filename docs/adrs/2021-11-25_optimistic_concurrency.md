# How to handle concurrency token columns in non–MSSQL databases

The `IOptimisticConcurrency` interface requires `DateTime DateModified` and `byte[] RowVersion` properties to be implemented by aggregate roots. However, the `byte[]` representation of `RowVersion` column is specific to Microsoft SQL Server and is not compatible with other databases, such as PostgreSQL and its convention to use `uint xmin` hidden column as a concurrency token.

## Status

Implemented (2021-12-06)

## Context

Until now, MSSQL was the only SQL database supported by CoreLib. This issue came up when we attempted to replace MSSQL Server with PostgreSQL in one of the projects.

We have considered two solutions to this problem: marking the existing interface as MSSQL–specific (plus perhaps introducing new ones for other databases) and removing `byte[] RowVersion` from the interface (keeping only `DateTime DateModified` with any row version properties staying only in shadow state).

## Decision

We have chosen to remove `byte[] RowVersion` from the interface and update builder methods like `IsOptimisticConcurrent` to optionally configure `byte[] RowVersion` as a shadow property. This allowed us to minimize pollution of domain interfaces with implementation–specific solutions that should have minimal impact on this layer.

## Consequences

* Project updating from previous versions will have to remove implementations of `byte[] IOptimisticConcurrency.RowVersion` property from aggregate roots. Updated builder methods will keep that property in shadow state, making migrations unnecessary.
* Having a property in shadow state will prevent tracked aggregate instances from being shared with other `DbContext`s. We consider this to be an upside :)

## Side notes

### How to use `IOptimisticConcurrency` with MSSQL

* Implement `IAggregateRoot` in your aggregate root entity as normal. Prefer using explicit interface implementation for `DateTime IOptimisticConcurrency.DateModified` to hide public setter from code that shouldn't use it.
* Configure your entity in `DbContext`'s `OnConfiguring` method like so:

```csharp
builder.Entity<Foo>().IsOptimisticConcurrent(addRowVersion: true /* the default, can be omitted */);
```

### How to use `IOptimisticConcurrency` with Npgsql

* Implement `IAggregateRoot` in your aggregate root entity as normal. Prefer using explicit interface implementation for `DateTime IOptimisticConcurrency.DateModified` to hide public setter from code that shouldn't use it.
* Configure your entity in `DbContext`'s `OnConfiguring` method like so:

```csharp
builder.Entity<Foo>().UseXminAsConcurrencyToken().IsOptimisticConcurrent(addRowVersion: false);
```
