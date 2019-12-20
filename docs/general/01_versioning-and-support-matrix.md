# Versioning

The library version is tricky. Since it is mostly used internally by us at LeanCode, we're not really following [Semantic Versioning](http://semver.org). Instead, we decide which version is considered stable and which one is under active development. There will also be versions that are unmaintained and should be no longer used.

Here is the list of available major versions of the library (as of 20-12-2019):

| CoreLib | .NET Core | Under development | Maintained | Notes         |
|---------|-----------|-------------------|------------|---------------|
| v4.1    | 3.1       | &#x2714;          | &#x2714;   | LTS           |
| v3.4    | 2.2       |                   |            | Not published |
|         |           |                   |            |               |

### A note on version numbers

We will try to match .NET Core major version with major version of our library. Currently that is not the case since we decided to bump major version some time ago (because we had different versioning scheme that didn't really work). We will catch up with .NET Core next year, when .NET 5 comes out. We don't think that we will match minor version of .NET though. That will constraint our ability to version this sensibly too much.
