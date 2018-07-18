#r "paket:
storage: none
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Xml
nuget Fake.Core.ReleaseNotes
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open System
open System.Threading.Tasks
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.DotNet

let MyGetAddress = "https://www.myget.org/F/leancode/api/v2/package"
let MyGetSymbolsAddress = "https://www.myget.org/F/leancode/symbols/api/v2/package"


let rootDir = Path.getFullName "."
let srcDir = Path.combine rootDir "src"
let packDir = Path.combine rootDir "packed"

let testProject = Path.combine rootDir "test/LeanCode.Tests.csproj"
let libVersionFile = Path.combine srcDir "targets/Version.targets"
let dependenciesFile = Path.combine srcDir "targets/Dependencies.props"

let configuration = Environment.environVarOrDefault "DOTNET_CONFIGURATION" "Release" |> DotNet.BuildConfiguration.Custom

let formatChangelog (changeLog: Changelog.Changelog) =
    let desc = defaultArg changeLog.LatestEntry.Description ""
    let changes = System.String.Join("\n", changeLog.LatestEntry.Changes)
    if String.IsNullOrWhiteSpace desc && String.IsNullOrWhiteSpace changes then ""
    else if String.IsNullOrWhiteSpace desc then "Changes:\n" + changes
    else if String.IsNullOrWhiteSpace changes then desc
    else desc + "\n\nChanges:\n" + changes

let updateChangelog (changeLog: Changelog.Changelog) =
    let buildNumber = uint32 <| Environment.environVar "BUILD_NUMBER"
    let newVersion = { changeLog.LatestEntry.SemVer with Patch = buildNumber }
    let newVerString = newVersion.ToString()
    let newEntry = Changelog.ChangelogEntry.New(changeLog.LatestEntry.AssemblyVersion, newVerString, [])
    { changeLog with Entries = newEntry :: changeLog.Entries }

Target.create "Restore" (fun _ ->
    Trace.trace "Restoring packages"
    DotNet.restore id ""
    DotNet.restore id testProject
)

Target.create "Build" (fun _ ->
    Trace.trace "Building packages"
    DotNet.build (fun c ->
        { c.WithCommon(DotNet.Options.withCustomParams (Some "--no-restore")) with
            Configuration = configuration
        }) ""
)

Target.create "Test" (fun _ ->
    let result = DotNet.exec id "msbuild" (testProject + " /t:RunTests")
    if not result.OK then failwith "Tests failed"
)

Target.create "UpdateVersion" (fun _ ->
    let changeLog = Changelog.load "CHANGELOG.md" |> updateChangelog
    let version = changeLog.LatestEntry.NuGetVersion

    Xml.pokeInnerText libVersionFile "/Project/PropertyGroup/Version" version
    Xml.pokeInnerText dependenciesFile "/Project/PropertyGroup/CoreLibVersion" version
    Xml.pokeInnerText libVersionFile "/Project/PropertyGroup/PackageReleaseNotes" (formatChangelog changeLog)
)

Target.create "Pack" (fun _ ->
    Trace.trace "Packaging"
    DotNet.pack (fun c ->
        { c with
            Configuration = configuration
            OutputPath = Some packDir})
        rootDir
)

Target.create "PublishToMyGet" (fun _ ->
    Trace.trace "Publishing NuGets..."
    let apiKey = Environment.environVar "NUGET_APIKEY"
    let files =
        !! (packDir + "/*.nupkg")
        -- (packDir + "/*.symbols.nupkg")
    let publish f =
        let opts = DotNet.Options.withRedirectOutput true
        let args = sprintf "\"%s\" -k %s -sk %s -s %s -ss %s" f apiKey apiKey MyGetAddress MyGetSymbolsAddress
        let res = DotNet.exec opts "nuget push" args
        if not res.OK then failwith (sprintf "Cannot upload %s" f)
    let pOpts = ParallelOptions()
    pOpts.MaxDegreeOfParallelism <- 1
    Parallel.ForEach(files, pOpts, publish) |> ignore
)

Target.create "Default" ignore

"Restore"
    ==> "Build"
    ==> "Test"
    ==> "Default"
    ==> "Pack"
    ==> "PublishToMyGet"

"UpdateVersion" ?=> "Restore"
"UpdateVersion" ==> "Pack"

#if !BOOTSTRAP
Target.runOrDefault "Default"
#endif
