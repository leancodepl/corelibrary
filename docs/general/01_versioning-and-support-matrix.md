Versioning
==========

CoreLib version is tricky. Since it is mostly used internally by us at LeanCode, we're not really following [Semantic Versioning](http://semver.org). Instead, we decide which version is considered stable but maintained, which one is under active development and which one is out of support. There will also be versions that are unmaintained and should be no longer used. There are some vague rules on how we decide what state particular version is in:

 1. If there are projects that are not actively worked on, it is maintained but not actively developed,
 2. If it is based on old (out of support) .NET Core version, it is unmaintained,
 3. If the version is used by active projects only, it is under active development.

Additionaly, there are some rules regarding versioning itself and changes to the version number:

 1. If major version of .NET Core changes, we bump major version of CoreLib,
 2. If minor version of .NET Core changes, we bump minor version,
 3. If CoreLib version is stable, we can't introduce breaking changes without changing version,
 4. We allow breaking changes between minor versions,
 5. If CoreLib version is under active development, we can introduce breaking changes without version bump,
 6. A single CoreLib version cannot be both stable and under active development,
 7. There is a small period of time after new CoreLib version is released when we allow all kinds of breakages (i.e. .NET Core bumps if we release during preview window).

## Library versions

All of the libraries that are part of the CoreLib are versioned together and require **exact** version of other libraries. This simplifies versioning substantially but at the expense of flexibility.

## Supported versions
Here is the list of available major versions of the library (as of 20-12-2019):

| CoreLib | .NET Core | Under development | Stable     | Notes         |
|---------|-----------|-------------------|------------|---------------|
| v4.1    | 3.1       | &#x2714;          |            | LTS           |
| v3.4    | 2.2       |                   | &#x2714;   | Not published |
|         |           |                   |            |               |

### A note on version numbers

We will try to match .NET Core major version with major version of our library. Currently that is not the case since we decided to bump major version some time ago (because we had different versioning scheme that didn't really work). We will catch up with .NET Core next year, when .NET 5 comes out. We don't think that we will match minor version of .NET though. That will constraint our ability to version this sensibly too much.
