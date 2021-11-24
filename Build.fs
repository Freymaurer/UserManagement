open Fake.Core
open Fake.IO
open Farmer
open Farmer.Builders

open Helpers

initializeContext()

let sharedPath = Path.getFullName "src/Shared"
let serverPath = Path.getFullName "src/Server"
let clientPath = Path.getFullName "src/Client"
let deployPath = Path.getFullName "deploy"
let sharedTestsPath = Path.getFullName "tests/Shared"
let serverTestsPath = Path.getFullName "tests/Server"
let clientTestsPath = Path.getFullName "tests/Client"

Target.create "Clean" (fun _ ->
    Shell.cleanDir deployPath
    run dotnet "fable clean --yes" clientPath // Delete *.fs.js files created by Fable
)

Target.create "InstallClient" (fun _ -> run npm "install" ".")

Target.create "Bundle" (fun _ ->
    [ "server", dotnet $"publish -c Release -o \"{deployPath}\"" serverPath
      "client", dotnet "fable -o output -s --run webpack -p" clientPath ]
    |> runParallel
)

Target.create "Azure" (fun _ ->
    let web = webApp {
        name "UserManagement"
        zip_deploy "deploy"
    }
    let deployment = arm {
        location Location.WestEurope
        add_resource web
    }

    deployment
    |> Deploy.execute "UserManagement" Deploy.NoParameters
    |> ignore
)

Target.create "certificates" (fun _ ->
    let isExisting = Fake.IO.File.allExist ["./.certs/localhost.pem"; "./.certs/localhost-key.pem"]
    if not isExisting then
        run mkcert_windows "-install localhost" "./.certs"
)

Target.create "Run" (fun _ ->
    let createCertsIfNotExisting =
        let isExisting = Fake.IO.File.allExist ["./.certs/localhost.pem"; "./.certs/localhost-key.pem"]
        if not isExisting then
            run mkcert_windows "-install localhost" "./.certs"
    let url = "https://localhost:8080"
    run dotnet "build" sharedPath
    [
        "client", dotnet "fable watch -o output -s --run webpack-dev-server --https" clientPath
        "server", dotnet "watch run" serverPath
        let _ =
            System.Threading.Thread.Sleep(System.TimeSpan(0,0,5))
            openBrowser url
        "database", dockerCompose "-f .\db\docker-compose.yaml -p safe-users up" ""
    ]
    |> runParallel
)

Target.create "RunTests" (fun _ ->
    run dotnet "build" sharedTestsPath
    [ "server", dotnet "watch run" serverTestsPath
      "client", dotnet "fable watch -o output -s --run webpack-dev-server --config ../../webpack.tests.config.js" clientTestsPath ]
    |> runParallel
)

Target.create "Format" (fun _ ->
    run dotnet "fantomas . -r" "src"
)

open Fake.Core.TargetOperators

let dependencies = [
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"
        ==> "Azure"

    "Clean"
        ==> "InstallClient"
        ==> "Run"

    "InstallClient"
        ==> "RunTests"
]

[<EntryPoint>]
let main args = runOrDefault args