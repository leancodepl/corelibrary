# LeanCode Core Library

LeanCode Core Library, or CoreLib for short, is a set of helper libraries developed at [our company](https://leancode.pl) that aids our day-to-day development. It also provides a general guidelines on how we should build our apps.

Our aim is to provide an opinionated framework for .NET Core app development. As for now we have, more-or-less, standardized:

* App startup, config & logging,
* Base DDD models & how it interacts with the rest of the framework,
* CQRS and CQRS-as-API,
* Basic localization,
* Some integrations with external services.

Even though is a framework, we try to stick to the ASP.NET Core model as close as possible and keep the "framework" part really small.

## Documentation

The CoreLib documentation is available here:

 1. [General](./general/README.md),
 2. [Basics](./basics/README.md),
 3. [Benchmarks](./benchmarks/README.md).
 4. [Domain](./domain/README.md).
 5. [Architecture decision records](./adrs/README.md).

## Domain Driven Design

LeanCode Core Library is strongly based on concepts of Domain Driven Design. If you are not familiar with this approach to developing software, you can check these books:

1. Domain-Driven Design: Tackling Complexity in the Heart of Software, Eric Evans
2. Implementing Domain-Driven Design, Vaughn Vernon