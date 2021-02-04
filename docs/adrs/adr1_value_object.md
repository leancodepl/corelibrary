# How to implement value objects in our domains

While modelling domains we use concept of value object. The issue is how to achieve model of value object described in DDD.

## Status

To be implemented (04.02.21)

## Context

We see two options: Base abstract class and record.

We used base class before records were introduced. The issue with base class is that it forced developer to implement `GetAttributesToIncludeInEqualityCheck` method in every implemented ValueObject and always contained all the properties that were defined.

When records came with C# 9.0 we decided to check how they suit our needs in regards to modeling value objects. In general, there were to differences in regards to previous option was that there were no abstract method that we need to implement (structural equality is more or less out of the box with records) and there were no marking interface or base class that clearly states that we are dealing with value object.

## Decision

We decided to use records that will inherit from abstract record `ValueObject`. This allows us to use all the benefits of records and do not loose the benefits of clearly stating what the record represents.

## Consequences

Abstract class was removed already. We need to provide implementation of that abstract record.

## Side notes

### How to use records as value objects

* All properties must have `private init` setters.
* Always remember that C# uses default value comparer (in case of arrays and collections this is reference quality).
* We can use `with` only in private context. If anywhere in your codebase you can do `vo with { A = "B" }` you violated first point.
* Use abstract base record (when it will be implemented).
