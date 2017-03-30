#I @"packages/FSharp.Data/lib/net40"

#r @"packages/FAKE/tools/FakeLib.dll"
#r @"packages/Fake.Azure.WebApps/lib/net451/Fake.Azure.WebApps.dll"

open System
open Fake
open Fake.NpmHelper
open Fake.ZipHelper
open Fake.BowerHelper
open Fake.EnvironmentHelper

let rootDir = "." |> FullName
let srcDir = rootDir @@ "src"
let testDir = rootDir @@ "test"
let deployDir = rootDir @@ "deploy"

let project = "LeanCode.Example"
let projectPath = srcDir @@ project

let loadDeploymentSettings () =
    Azure.WebApps.readSiteSettingsFromEnv (fun s ->
        { s with DeployPath = "site/wwwroot" })

let isTeamCityBuild = not isLocalBuild && buildServer = BuildServer.TeamCity

let getToolPath name =
    let fileName = if isUnix then name else name + ".cmd"
    defaultArg (tryFindFileOnPath fileName) name

let npmPath = getToolPath "npm"
let bowerPath = getToolPath "bower"
let gulpPath = getToolPath "gulp"

let configuration = getBuildParamOrDefault "configuration" "Release"
let cleanDatabase = hasBuildParam "cleandb"
let dontMigrateDatabase = hasBuildParam "nodb"

let Gulp workingDir target =
    use t = traceStartTaskUsing "Gulp" (sprintf "Target: %s" target)
    let ok =
        execProcess (fun info ->
            info.FileName <- gulpPath |> FullName
            info.WorkingDirectory <- workingDir
            info.Arguments <- target) TimeSpan.MaxValue
    if not ok then failwith (sprintf "'gulp %s' task failed" target)

let restoreFrontend projDir =
    Npm (fun p ->
            { p with
                Command = NpmHelper.Install NpmHelper.Standard
                WorkingDirectory = projDir
                NpmFilePath = npmPath })
    Bower (fun p ->
            { p with
                Command = BowerHelper.Install BowerHelper.Standard
                WorkingDirectory = projDir
                BowerFilePath = bowerPath })

let publishApp zipFile settingsLoader =
    let settings = settingsLoader ()
    let config = Azure.WebApps.acquireCredentials settings

    Azure.WebApps.stopDotNetCoreAppAndWait config
    Azure.WebApps.pushZipFile config zipFile
    Azure.WebApps.startWebApp config

Target "Clean" (fun () ->
    !! (srcDir @@ "*/bin")
    ++ (srcDir @@ "*/obj")
    ++ deployDir
        |> CleanDirs

    CleanDir <| deployDir @@ project
    DeleteFile <| deployDir @@ project + ".zip"
)

Target "Restore" (fun () ->
    trace "Restoring packages"
    DotNetCli.Restore (fun c -> { c with WorkingDir = rootDir })
    // restoreFrontend projectPath
)

Target "Build" (fun () ->
    trace "Building projects"
    DotNetCli.Build (fun c -> { c with Configuration = configuration; Project = projectPath })
)

Target "PublishApps" (fun () ->
    let outPath = deployDir @@ project
    DotNetCli.Publish (fun c -> { c with Configuration = configuration; Project = projectPath; Output = outPath })
)

Target "ZipApps" (fun () ->
    let outPath = deployDir @@ project
    let outZip = outPath + ".zip"

    !! (outPath @@ "**") |> Zip outPath outZip
)

Target "PublishArtifacts" (fun () ->
    let outPath = deployDir @@ project
    let outZip = outPath + ".zip"
    let artifactFile =
        sprintf "%s-%s.zip" outPath <| Git.Information.getCurrentHash ()
    CopyFile artifactFile outZip
    TeamCityHelper.PublishArtifact artifactFile
)

Target "Publish" (fun () ->
    let zipFile = deployDir @@ project + ".zip"
    publishApp zipFile loadDeploymentSettings
)

Target "PublishDatabase" (fun () ->
    try
        if cleanDatabase then
            trace "Reverting all migrations"
            DotNetCli.RunCommand (fun c -> { c with WorkingDir = projectPath }) "ef database update 0"
        trace "Updating database"
        DotNetCli.RunCommand (fun c -> { c with WorkingDir = projectPath }) "ef database update"
    with
        | e -> traceImportant <| "Cannot update database (" + e.Message + "), ignoring error"
)

Target "Default" DoNothing

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Default"
    ==> "PublishApps"
    ==> "ZipApps"
    =?> ("PublishArtifacts", isTeamCityBuild)
    =?> ("PublishDatabase", not dontMigrateDatabase)
    ==> "Publish"

RunTargetOrDefault "Default"
