# Value Objects

While modelling a domain we use the concept of value objects. The issue is how to represent value objects in code, as described in DDD.

## Status

Implemented (05.02.21)

## Context

We see two options: base abstract class and record.

We used base class before records were introduced. The issue with base class is that it forces developers to implement `GetAttributesToIncludeInEqualityCheck` method in every implemented `ValueObject`.

When records came in C# 9.0 we decided to check how they suit our needs in regards to modeling value objects. In general, there were two differences compared to previous option: there were no abstract method that we need to implement (structural equality is more or less out of the box with records) and there were no marker interfaces or base classes that clearly state that we are dealing with a value object.

## Decision

We decided to use records that will inherit from an abstract record `ValueObject`. This allows us to use all the benefits of records and do not lose the benefits of clearly stating what the record represents.

## Consequences

Abstract class will be removed. We need to provide implementation of that abstract record.

## Side notes

### How to use records as value objects

* All properties must have `private init` setters.
* Always remember that C# uses default value comparator (in case of arrays and collections this is reference equality).
* We can use `with` only in private context. If anywhere in your codebase you can do `vo with { A = "B" }` you violated first point.
* Use the abstract base record (when it will be implemented).
