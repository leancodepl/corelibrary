#r @"packages/FAKE/tools/FakeLib.dll"

open System
open Fake

let rootDir = "." |> FullName
let srcDir = rootDir @@ "src"
let testDir = rootDir @@ "test"

let configuration = getBuildParamOrDefault "configuration" "Release"

Target "Clean" (fun () ->
    !! (srcDir @@ "*/bin")
    ++ (srcDir @@ "*/obj")
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

Target "Pack" DoNothing

Target "Default" DoNothing

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Test"
    ==> "Default"
    ==> "Pack"

RunTargetOrDefault "Default"
