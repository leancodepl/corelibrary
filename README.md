# LeanCode CoreLibrary

![Corelibrary Build & Publish](https://github.com/leancodepl/corelibrary/workflows/Corelibrary%20Build%20&%20Publish/badge.svg)
![Nuget](https://img.shields.io/nuget/vpre/LeanCode.Components)
![Feedz](https://img.shields.io/feedz/v/leancode/public/LeanCode.Components)
[![codecov](https://codecov.io/gh/leancodepl/corelibrary/branch/v5.0/graph/badge.svg?token=ROFNA3WTTD)](https://codecov.io/gh/leancodepl/corelibrary)
[![License](https://img.shields.io/badge/License-Apache_2.0-green.svg)](https://www.apache.org/licenses/LICENSE-2.0)

The LeanCode Core Library is a set of helper libraries developed at [our company](https://leancode.co/) that aids our day-to-day development. Not only does it serve as a facilitator in our day-to-day coding activities, but it also encapsulates comprehensive guidelines, gathers our collective knowledge on application architecture and development best practices.

Our primary objective is to provide a definitive and opinionated framework tailored for .NET Core application development. Within this framework, we've standardized various facets of application design and implementation:

* **CQRS (Command Query Responsibility Segregation) and CQRS-as-API:** Establishing conventions and practices for implementing CQRS patterns and their representation through APIs.

* **Domain-Driven Design (DDD) Models:** Defining foundational DDD models and integrating them within the broader framework.

* **Integrations with External Services:** Pre-defined integrations and guidelines for interfacing with external services, ensuring consistency and reliability.

* **Additional Functionalities:** Including features such as handling audit logs, facilitating force updates, and localization.

* **Tests:** Helpers dedicated to aiding and improving the process of writing and executing tests.

While embodying the attributes of a framework, LeanCode Core Library aligns closely with the ASP.NET Core model, emphasizing modularity and minimal intrusion into the application codebase.

## Documentation

CoreLibrary documentation is available [here](https://leancode-corelibrary.readthedocs.io/).

## Versioning

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
| v8.0    | 8.0       | &#x2714;          | &#x2714;   |                   |

## Building & Testing

### Building

```sh
dotnet build
```

#### Release version

If you want to build release configuration of the library, you need to specify what version the output package will have. That can be done with `VERSION` environment variable or by passing `VERSION` as MSBuild parameter to the `build` command. CI also specifies `GIT_COMMIT` that is appended to `InformationalVersion` property of the assemblies to mark exact source code.

### Testing

The framework can be unit-tested by `cd`ing into `test` folder and calling

```sh
dotnet msbuild /t:RunTests
```

Moreover, there are some integration-style tests that require external services. They can be tested with `docker` and `docker-compose` tools. Currently there is one integration-test suite:

 1. `test/LeanCode.IntegrationTests`,

It has a `docker` folder that contains necessary configuration. You can run the suite using:

```sh
docker-compose run test
```

### Publishing

After successful test, packages can be packed with

```sh
dotnet pack -c Release -o $PWD/publish
```

and then published to NuGet feed with

```sh
dotnet nuget push 'publish/*.nupkg'
```

provided that API Key is correctly specified in profile/machine `NuGet.Config`.

## Project structure

### Root structure

The project is divided into the main directories:

 1. `src` with the source code,
 2. `test` with test,
 3. `benchmarks` with benchmarking project,
 4. `docs` with this documentation.

Plus there are some files in the root directory (SLN, config files & READMEs).

#### Source code structure

The `src` folder that contains the main source code is then divided into:

 1. `Core` - the bootstrapping part of the CoreLib,
 2. `Domain` - domain model-related projects,
 3. `Infrastructure` - infrastructure-related projects,
 4. `Helpers` - really small helper projects,
 5. `Testing` - testing helpers (e.g. integration tests),
 6. `Tools` - projects that enhance build-time (e.g. contracts gen, .NET Core analyzers).

`test` folder follows `str` structure closely.

### Build system

CoreLib build system mostly MSBuild-based, with some help of CI system to orchestrate build/test/publish process (see [Building & Testing](./building_and_testing.md) for more details).

We leverage .NET Core's MSBuild `Directory.Build.targets` files to centrally manage dependency versions. It is forbidden to directly specify `Version` in `csproj`s. Instead, one adds simple `<ProjectReference Include="NAME" />` and then `<ProjectReference Update="NAME" Version="VALID_VERSION" />` in `Directory.Build.targets` in the CoreLib root. This immensely helps avoiding dependency conflicts down the road.

Besides `.targets` file, we use central `Directory.Build.props` to manage some of the project properties. Check [/Directory.Build.props], [/src/Directory.Build.props] and [/test/Directory.Build.props] what is being centrally set.

### Creating new packages

Creating new packages (that will be published to the feed) is simple. You just have to:

1. Create new .NET CoreLibrary project in the correct location,
2. Remove `TargetFramework` since it is managed externally.

Or you can just modify the following project template (most of the projects use this):

```xml
<Project Sdk="Microsoft.NET.Sdk">

</Project>
```

[/Directory.Build.props]: https://github.com/leancodepl/corelibrary/blob/HEAD/Directory.Build.props
[/src/Directory.Build.props]: https://github.com/leancodepl/corelibrary/blob/HEAD/src/Directory.Build.props
[/test/Directory.Build.props]: https://github.com/leancodepl/corelibrary/blob/HEAD/test/Directory.Build.props
