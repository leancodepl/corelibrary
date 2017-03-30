LeanCode Core Library
=====================

### Versioning

These packages have to be correctly versioned. Use [Semantic Versioning](http://semver.org), i.e. if you modify (change/remove) something from the public API (make breaking change), you have to bump major version, otherwise change in minor/patch version is enough. Also - write down all changes in the [changelog](CHANGELOG.md).

Change in required dependencies (major version bump) may be also considered a breaking change.

### Referencing packages

Try not to reference too many NuGets in the core library projects, as this will make versioning harder. There are packages "common" to all infrastructure-related projects, that is:

 - Autofac
 - AutoMapper
 - Serilog

They change frequently, and sometimes we really want them to be up-to-date, so they are in separate `.targets` file - if you want to reference any of them, reference all of them or discuss this with the whole team (i.e. `.targets` file split).

There may be more NuGets that are shared between projects that have not been identified yet. If you find one - report it.

There's also problem with ASP.NET Core MVC - we want to have it up-to-date, but it has many, many packages that are not required for all of the project. Hence the `Mvc.targets` file that just binds the versions. Each package is responsible for referencing the required set of MVC-related packages.
