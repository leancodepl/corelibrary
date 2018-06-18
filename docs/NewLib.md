### Creating new packages

Creating new packages (that will be published to company's MyGet) is simple. You just have to:

1. Create new .NET Core project in the correct location,
2. Remove `PackageId` and `Version` elements from the `.csproj`,
3. Specify `TargetFramework` and `AssemblyName`,
3. Import file `src/targets/Lib.targets` in the `.csproj`.

Or you can just modify the following project template (most of the projects use this):

```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <AssemblyName>LeanCode.(...)</AssemblyName>
  </PropertyGroup>
  <Import Project="src/targets/Lib.targets" />

  <ItemGroup>
    <ProjectReference Include="(...)" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="(...)" Version="(...)" />
  </ItemGroup>

</Project>
```

Everything else will be handled by `build.fsx` automatically.
