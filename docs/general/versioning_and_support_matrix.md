# Versioning

CoreLib version is tricky. Since it is mostly used internally by us at LeanCode, we're not really following [Semantic Versioning](http://semver.org). Instead, we decide which version is considered stable but maintained, which one is under active development and which one is out of support. There will also be versions that are unmaintained and should be no longer used. There are some vague rules on how we decide what state particular version is in:

 1. If there are projects that are not actively worked on, it is maintained but not actively developed,
 2. If it is based on old (out of support) .NET Core version, it is unmaintained,
 3. If the version is used by active projects only, it is under active development.

Additionally, there are some rules regarding versioning itself and changes to the version number:

 1. Since v5, CoreLib major version is the same as .NET major version,
 2. Minor version is used as a major version,
 3. If CoreLib version is stable, we can't introduce breaking changes without changing version,
 4. We allow breaking changes between minor (major) versions,
 5. If CoreLib version is under active development, we can introduce breaking changes without version bump,
 6. A single CoreLib version cannot be both stable and under active development,
 7. There is a small period of time after new CoreLib version is released when we allow all kinds of breakages (i.e. .NET Core bumps if we release during preview window).

## Library versions

All of the libraries that are part of the CoreLib are versioned together and require **exact** version of other libraries. This simplifies versioning substantially but at the expense of flexibility.

## Supported versions

Here is the list of available major versions of the library (as of 2022-03-25):

| CoreLib | .NET Core | Under development | Stable     | Notes             |
|---------|-----------|-------------------|------------|-------------------|
| v3.4    | 2.2       |                   |            | Not published     |
| v4.1    | 3.1       |                   |            | Unmaintained      |
| v4.2    | 3.1       |                   |            | Unmaintained      |
| v5.0    | 5.0       |                   |            | Unmaintained      |
| v5.1    | 5.0       |                   |            | Unmaintained      |
| v6.0    | 6.0       |                   |            | Unmaintained      |
| v6.1    | 6.0       |                   |            | Unmaintained      |
| v7.0    | 7.0       |                   | &#x2714;   |                   |
| v8.0    | 8.0       | &#x2714;          |            | In preview        |
