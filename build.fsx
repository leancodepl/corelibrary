#r @"packages/FAKE/tools/FakeLib.dll"

open System
open Fake

let rootDir = "." |> FullName
let srcDir = rootDir @@ "src"
let testDir = rootDir @@ "test"
let packDir = rootDir @@ "packed"

let libVersionFile = srcDir @@ "targets/Lib.targets"

let configuration = getBuildParamOrDefault "configuration" "Release"

let changeLog = ChangeLogHelper.LoadChangeLog "CHANGELOG.md"
let version = changeLog.LatestEntry.NuGetVersion

let formatChangelog () =
    let desc = defaultArg changeLog.LatestEntry.Description ""
    let changes = System.String.Join("\n", changeLog.LatestEntry.Changes)
    if String.IsNullOrWhiteSpace desc && String.IsNullOrWhiteSpace changes then ""
    else if String.IsNullOrWhiteSpace desc then "Changes:\n" + changes
    else if String.IsNullOrWhiteSpace changes then desc
    else desc + "\n\nChanges:\n" + changes

Target "Clean" (fun () ->
    !! (srcDir @@ "*/bin")
    ++ (srcDir @@ "*/obj")
    ++ packDir
        |> CleanDirs
)

Target "Restore" (fun () ->
    trace "Restoring packages"
    DotNetCli.Restore (fun c -> { c with WorkingDir = rootDir })
)

Target "Build" (fun () ->
    trace "Building packages"
    DotNetCli.Build (fun c -> { c with WorkingDir = rootDir; Configuration = configuration })
)

Target "Test" (fun () ->
    trace "Testing"
    !! (testDir @@ "**/*.csproj")
        |> Seq.iter (fun p -> DotNetCli.Test (fun c -> { c with WorkingDir = rootDir; Project = p }))
)

Target "UpdateVersion" (fun () ->
    XmlPokeInnerText libVersionFile "/Project/PropertyGroup/Version" version
    XmlPokeInnerText libVersionFile "/Project/PropertyGroup/PackageReleaseNotes" (formatChangelog ())
)

Target "Pack" (fun () ->
    trace "Packaging"
    DotNetCli.Pack (fun c -> { c with WorkingDir = rootDir; Configuration = configuration; OutputPath = packDir })
)

Target "PublishToMyGet" DoNothing

Target "Release" (fun () ->
    let tagName = "v" + version

    Git.Staging.StageAll ""
    Git.Commit.Commit "" <| sprintf "Bump version to %s" version
    Git.Branches.tag "" tagName

    Git.Branches.push ""
    Git.Branches.pushTag "" "origin" tagName
)

Target "Default" DoNothing

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Test"
    ==> "Default"
    ==> "UpdateVersion"
    ==> "Pack"
    ==> "PublishToMyGet"

"UpdateVersion" ==> "Release"

RunTargetOrDefault "Default"
