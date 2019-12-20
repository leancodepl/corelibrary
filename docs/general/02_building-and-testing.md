# Building & Testing

## Building

```sh
dotnet build
```

### Release version

If you want to build release configuration of the library, you need to specify what version the output package will have. That can be done with `VERSION` environment variable or by passing `VERSION` as MSBuild parameter to the `build` command. CI also specifies `GIT_COMMIT` that is appended to `InformationalVersion` property of the assemblies.

### MSBuild structure

## Testing

The framework can be unit-tested by calling `dotnet msbuild /t:RunTests` in the `test` folder.

Moreover, there are some integration-style tests that require external services. They can be tested with `docker` and `docker-compose` tool. There are two integration-test suites:

 1. `test/IntegrationTests` - _real_ integration tests,
 2. `test/Testing/LeanCode.IntegrationTestHelpers.Tests` - test for the integration test helpers.

Both have `docker` folder that contains necessary configuration. You can run each of the suites using:
```sh
docker-compose run test
```

## Creating new packages

Creating new packages (that will be published to MyGet) is simple. You just have to:

1. Create new .NET Core library project in the correct location,
2. Remove `TargetFramework` since it is specified centrally,

Or you can just modify the following project template (most of the projects use this):

```
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="(...)" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="(...)" />
  </ItemGroup>

</Project>
```

Everything else will be handled by the build system automatically.
