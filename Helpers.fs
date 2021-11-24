module Helpers

open Fake.Core
open System.Runtime.InteropServices

let initializeContext () =
    let execContext = Context.FakeExecutionContext.Create false "build.fsx" [ ]
    Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

module Proc =
    module Parallel =
        open System

        let locker = obj()

        let colors =
            [|  ConsoleColor.Yellow
                ConsoleColor.Blue
                ConsoleColor.Magenta
                ConsoleColor.Cyan
                ConsoleColor.DarkBlue
                ConsoleColor.DarkYellow
                ConsoleColor.DarkMagenta
                ConsoleColor.DarkCyan |]

        let print color (colored: string) (line: string) =
            lock locker
                (fun () ->
                    let currentColor = Console.ForegroundColor
                    Console.ForegroundColor <- color
                    Console.Write colored
                    Console.ForegroundColor <- currentColor
                    Console.WriteLine line)

        let onStdout index name (line: string) =
            let color = colors.[index % colors.Length]
            if isNull line then
                print color $"{name}: --- END ---" ""
            else if String.isNotNullOrEmpty line then
                print color $"{name}: " line

        let onStderr name (line: string) =
            let color = ConsoleColor.Red
            if isNull line |> not then
                print color $"{name}: " line

        let redirect (index, (name, createProcess)) =
            createProcess
            |> CreateProcess.redirectOutputIfNotRedirected
            |> CreateProcess.withOutputEvents (onStdout index name) (onStderr name)

        let printStarting indexed =
            for (index, (name, c: CreateProcess<_>)) in indexed do
                let color = colors.[index % colors.Length]
                let wd =
                    c.WorkingDirectory
                    |> Option.defaultValue ""
                let exe = c.Command.Executable
                let args = c.Command.Arguments.ToStartInfo
                print color $"{name}: {wd}> {exe} {args}" ""

        let run cs =
            cs
            |> Seq.toArray
            |> Array.indexed
            |> fun x -> printStarting x; x
            |> Array.map redirect
            |> Array.Parallel.map Proc.run

let createProcess exe arg dir =
    CreateProcess.fromRawCommandLine exe arg
    |> CreateProcess.withWorkingDirectory dir
    |> CreateProcess.ensureExitCode

let dotnet = createProcess "dotnet"
let dockerCompose = createProcess "docker-compose"
let npm =
    let npmPath =
        match ProcessUtils.tryFindFileOnPath "npm" with
        | Some path -> path
        | None ->
            "npm was not found in path. Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
            |> failwith

    createProcess npmPath

let mkcert_windows =
    let mkcertLink = @"https://github.com/FiloSottile/mkcert#installation"
    if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
        createProcess "./.certs/mkcert_windows.exe"
    else
        failwith $"Cannot create local certificates. To check how to install local certificates pls visit: {mkcertLink}.
        Copy path to created certs in webpack.config.js -> module.exports -> devServer -> https."

///Choose process to open Browser with depending on OS. Thanks to @zyzhu for hinting at a solution (https://github.com/plotly/Plotly.NET/issues/31)
let openBrowser url =
    if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
        CreateProcess.fromRawCommand "cmd.exe" [ "/C"; $"start {url}" ] |> Proc.run |> ignore
    elif RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then
        CreateProcess.fromRawCommand "xdg-open" [ url ] |> Proc.run |> ignore
    elif RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then
        CreateProcess.fromRawCommand "open" [ url ] |> Proc.run |> ignore
    else
        failwith "Cannot open Browser. OS not supported."

let run proc arg dir =
    proc arg dir
    |> Proc.run
    |> ignore

let runParallel processes =
    processes
    |> Proc.Parallel.run
    |> ignore

let runOrDefault args =
    try
        match args with
        | [| target |] -> Target.runOrDefault target
        | _ -> Target.runOrDefault "Run"
        0
    with e ->
        printfn "%A" e
        1
