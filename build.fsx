#r @"packages/FAKE/tools/FakeLib.dll"

open System
open Fake

let rootDir = "." |> FullName
let srcDir = rootDir @@ "src"
let testDir = rootDir @@ "test"
let packDir = rootDir @@ "packed"

let libVersionFile = srcDir @@ "targets/Version.targets"
let dependenciesFile = srcDir @@ "Core/LeanCode.Targets/Dependencies.props"

let configuration = getBuildParamOrDefault "configuration" "Release"

let formatChangelog (changeLog: ChangeLogHelper.ChangeLog) =
    let desc = defaultArg changeLog.LatestEntry.Description ""
    let changes = System.String.Join("\n", changeLog.LatestEntry.Changes)
    if String.IsNullOrWhiteSpace desc && String.IsNullOrWhiteSpace changes then ""
    else if String.IsNullOrWhiteSpace desc then "Changes:\n" + changes
    else if String.IsNullOrWhiteSpace changes then desc
    else desc + "\n\nChanges:\n" + changes

let updateChangelog (changeLog: ChangeLogHelper.ChangeLog) =
    let branch = Git.Information.getBranchName ""
    if branch = "master" then changeLog
    else
        let commits = Git.CommandHelper.runSimpleGitCommand "" ("rev-list HEAD --count")
        let newVersion = { changeLog.LatestEntry.SemVer with Minor = changeLog.LatestEntry.SemVer.Minor + 1; Patch = 0 }
        let newVerString = newVersion.ToString() + "-alpha." + commits
        if Option.isSome changeLog.Unreleased
        then changeLog.PromoteUnreleased newVerString
        else
            let newEntry = ChangeLogHelper.ChangeLogEntry.New(changeLog.LatestEntry.AssemblyVersion, newVerString, [])
            { changeLog with Entries = newEntry :: changeLog.Entries }

let changeLog = ChangeLogHelper.LoadChangeLog "CHANGELOG.md" |> updateChangelog
let version = changeLog.LatestEntry.NuGetVersion

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
        |> Seq.map DirectoryName
        |> Seq.iter (fun p ->
            DotNetCli.RunCommand (fun c -> { c with WorkingDir = p }) "xunit -nobuild")
)

Target "UpdateVersion" (fun () ->
    XmlPokeInnerText libVersionFile "/Project/PropertyGroup/Version" version
    XmlPokeInnerText dependenciesFile "/Project/PropertyGroup/CoreLibVersion" version
    XmlPokeInnerText libVersionFile "/Project/PropertyGroup/PackageReleaseNotes" (formatChangelog changeLog)
)

Target "Pack" (fun () ->
    trace "Packaging"
    DotNetCli.Pack (fun c -> { c with WorkingDir = rootDir; Configuration = configuration; OutputPath = packDir })
)

Target "PublishToMyGet" (fun () ->
    trace "Publishing NuGets..."
    Paket.Push (fun cfg ->
        { cfg with
            WorkingDir = packDir
            PublishUrl = "https://www.myget.org/F/leancode"
            DegreeOfParallelism = 0
        })
)

Target "Release" (fun () ->
    let branch = Git.Information.getBranchName ""
    if branch <> "master" then failwith "You can make a release only from master branch"

    let tagName = "v" + version

    Git.Staging.StageAll ""
    Git.Commit.Commit "" <| sprintf "Bump version to %s" version
    Git.Branches.tag "" tagName

    Git.Branches.push ""
    Git.Branches.pushTag "" "origin" tagName
)

Target "Default" DoNothing

"Restore"
    ==> "Build"
    ==> "Test"
    ==> "Default"
    ==> "Pack"
    ==> "PublishToMyGet"

"UpdateVersion" ?=> "Restore"
"UpdateVersion" ==> "Pack"
"UpdateVersion" ==> "Release"

RunTargetOrDefault "Default"
