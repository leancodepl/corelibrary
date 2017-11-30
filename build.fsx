#r @"packages/FAKE/tools/FakeLib.dll"

open System
open Fake.Core
open Fake.Core.TargetOperators
open Fake.Core.Globbing.Operators
open Fake.IO
open Fake.DotNet
open Fake.DotNet.Cli
open Fake.EnvironmentHelper
open Fake.Tools
open Fake.ChangeLogHelper
open Fake.FileHelper
open Fake.ProcessHelper

let rootDir = Path.getFullName "."
let srcDir = Path.combine rootDir "src"
let testDir = Path.combine rootDir "test"
let packDir = Path.combine rootDir "packed"

let libVersionFile = Path.combine srcDir "targets/Version.targets"
let dependenciesFile = Path.combine srcDir "Core/LeanCode.Targets/Dependencies.props"

let configuration = environVarOrDefault "DOTNET_CONFIGURATION" "Release" |> BuildConfiguration.Custom

let formatChangelog (changeLog: ChangeLog) =
    let desc = defaultArg changeLog.LatestEntry.Description ""
    let changes = System.String.Join("\n", changeLog.LatestEntry.Changes)
    if String.IsNullOrWhiteSpace desc && String.IsNullOrWhiteSpace changes then ""
    else if String.IsNullOrWhiteSpace desc then "Changes:\n" + changes
    else if String.IsNullOrWhiteSpace changes then desc
    else desc + "\n\nChanges:\n" + changes

let updateChangelog (changeLog: ChangeLog) =
    let branch = Git.Information.getBranchName ""
    if branch = "master" then changeLog
    else
        let commits = Git.CommandHelper.runSimpleGitCommand "" ("rev-list HEAD --count")
        let newVersion = { changeLog.LatestEntry.SemVer with Minor = changeLog.LatestEntry.SemVer.Minor + 1; Patch = 0 }
        let newVerString = newVersion.ToString() + "-alpha." + commits
        if Option.isSome changeLog.Unreleased
        then changeLog.PromoteUnreleased newVerString
        else
            let newEntry = ChangeLogEntry.New(changeLog.LatestEntry.AssemblyVersion, newVerString, [])
            { changeLog with Entries = newEntry :: changeLog.Entries }

let changeLog = LoadChangeLog "CHANGELOG.md" |> updateChangelog
let version = changeLog.LatestEntry.NuGetVersion

DefaultDotnetCliDir <-
    let name = "dotnet"
    let fileName = if isUnix then name else name + ".exe"
    Path.getDirectory <| defaultArg (tryFindFileOnPath fileName) name

Target.Create "Clean" (fun _ ->
    !! (srcDir @@ "*/bin")
    ++ (srcDir @@ "*/obj")
    ++ packDir
    |> CleanDirs
)

Target.Create "CleanDeploy" (fun _ ->
    CleanDirs [packDir]
)

Target.Create "Restore" (fun _ ->
    Trace.trace "Restoring packages"
    DotnetRestore id ""
)

Target.Create "Build" (fun _ ->
    Trace.trace "Building packages"
    DotnetCompile (fun c ->
        { c with
            Configuration = configuration
            Common =
                { DotnetOptions.Default with
                   CustomParams = Some "--no-restore" }
        }) ""
)

Target.Create "Test" (fun _ ->
    Trace.trace "Testing"
    !! (testDir @@ "**/*.csproj")
    |> Seq.map Path.getDirectory
    |> Seq.iter (fun p ->
        Dotnet { DotnetOptions.Default with WorkingDirectory = p } "xunit" |> ignore)
)

Target.Create "UpdateVersion" (fun _ ->
    Xml.PokeInnerText libVersionFile "/Project/PropertyGroup/Version" version
    Xml.PokeInnerText dependenciesFile "/Project/PropertyGroup/CoreLibVersion" version
    Xml.PokeInnerText libVersionFile "/Project/PropertyGroup/PackageReleaseNotes" (formatChangelog changeLog)
)

Target.Create "Pack" (fun _ ->
    Trace.trace "Packaging"
    DotnetPack (fun c ->
        { c with
            Configuration = configuration
            OutputPath = Some packDir})
        rootDir
)

Target.Create "PublishToMyGet" (fun _ ->
    Trace.trace "Publishing NuGets..."
    Paket.Push (fun cfg ->
        { cfg with
            WorkingDir = packDir
            PublishUrl = "https://www.myget.org/F/leancode"
            DegreeOfParallelism = 0
        })
)

Target.Create "Default" Target.DoNothing

"CleanDeploy"
    ==> "Restore"
    ==> "Build"
    ==> "Test"
    ==> "Default"
    ==> "Pack"
    ==> "PublishToMyGet"

"UpdateVersion" ?=> "Restore"
"UpdateVersion" ==> "Pack"

Target.RunOrDefault "Default"
