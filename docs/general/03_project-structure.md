# Project structure

## Root structure

The project is divided into the main directories:

 1. `src` with the source code,
 2. `test` with test,
 3. `benchmarks` with benchmarking project,
 4. `docs` with this documentation.

Plus there are some files in the root directory (SLN, config files & READMEs).

### Source code structure

The `src` folder that contains the main source code is then divided into:

 1. `Core` - the bootstrapping part of the CoreLib,
 2. `Domain` - domain model-related projects,
 3. `Infrastructure` - infrastructure-related projects,
 4. `Helpers` - really small helper projects,
 5. `Testing` - testing helpers (e.g. integration tests),
 6. `Tools` - projects that enhance build-time (e.g. contracts gen, .NET Core analyzers).

`test` folder follows `str` structure closely.

## Build system

CoreLib build system mostly MSBuild-based, with some help of CI system to orchestrate build/test/publish process (see [Building & Testing](./02_building-and-testing.md) for more details).

We leverage .NET Core's MSBuild `Directory.Build.targets` files to centrally manage dependency versions. It is forbidden to directly specify `Version` in `csproj`s. Instead, one adds simple `<ProjectReference Include="NAME" />` and then `<ProjectReference Update="NAME" Version="VALID_VERSION" />` in `Directory.Build.targets` in the CoreLib root. This immensely helps avoiding dependency conflicts down the road.

Besides `.targets` file, we use central `Directory.Build.props` to manage some of the project properties. Check [/Directory.Build.props](../../Directory.Build.props), [/src/Directory.Build.props](../../src/Directory.Build.props) and [/test/Directory.Build.props](../../test/Directory.Build.props) what is being centrally set.

## Creating new packages

Creating new packages (that will be published to the feed) is simple. You just have to:

1. Create new .NET Core library project in the correct location,
2. Remove `TargetFramework` since it is managed externally.

Or you can just modify the following project template (most of the projects use this):

```xml
<Project Sdk="Microsoft.NET.Sdk">

</Project>
```

:)
