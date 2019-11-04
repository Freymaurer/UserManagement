open System
open System.IO
open Saturn
open Shared

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Giraffe

open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.EntityFrameworkCore
open FSharp.Control.Tasks

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

module DotnetModule =

    open System.Text

    let showErrors (errors : IdentityError seq) =
        errors
        |> Seq.fold (fun acc err ->
            sprintf "Code: %s, Description: %s" err.Code err.Description
            |> acc.AppendLine : StringBuilder) (StringBuilder(""))
        |> (fun x -> x.ToString())

    let dotnetLogin (user:LoginModel) (context: HttpContext) =
        task {
            let signInManager = context.GetService<SignInManager<IdentityUser>>()
            let! result = signInManager.PasswordSignInAsync(user.Username, user.Password, true, false)
            match result.Succeeded with
            | true ->
               return LoginSuccess (result.ToString())
            | false -> return LoginFail (result.ToString())
        } |> fun x -> x.Result

    let dotnetGetUser (context: HttpContext) =
        task {
            let userManager = context.GetService<UserManager<IdentityUser>>()
            let! user = userManager.GetUserAsync context.User
            return { Username = user.UserName; Email = user.Email }
        } |> fun x -> x.Result

    let dotnetUserLogOut (context: HttpContext) =
        task {
            let signInManager = context.GetService<SignInManager<IdentityUser>>()
            do! signInManager.SignOutAsync()
            return LogoutSuccess "Log Out Success"
        } |> fun x -> x.Result

    let dotnetRegistration (registerModel:RegisterModel) (context: HttpContext) =

        task {
            let  user        = IdentityUser(UserName = registerModel.Username, Email = registerModel.Email)
            let  userManager = context.GetService<UserManager<IdentityUser>>()
            let! result      = userManager.CreateAsync(user, registerModel.Password)
            match result.Succeeded with
            | false -> return (RegisterFail (showErrors result.Errors))
            | true  ->
                let signInManager = context.GetService<SignInManager<IdentityUser>>()
                do! signInManager.SignInAsync(user, true)
                return RegisterSuccess "Registration Successful"
        } |> fun x -> x.Result

let dotnetApi (context: HttpContext) = {
    dotnetLogin = fun (loginModel) -> async { return (DotnetModule.dotnetLogin loginModel context) }
    dotnetRegister = fun (registerModel) -> async { return (DotnetModule.dotnetRegistration registerModel context) }
}

let nice() =
    { Value = 69 }

let dotnetSecureApi (context: HttpContext) = {
    dotnetGetUser = fun () -> async { return (DotnetModule.dotnetGetUser context)}
    dotnetUserLogOut = fun () -> async { return (DotnetModule.dotnetUserLogOut context) }
    getUserCounter = fun () -> async { return nice() }
}

let counterApi = {
    initialCounter = fun () -> async {return { Value = 42 }}
}

open Microsoft.AspNetCore.Builder

///https://github.com/giraffe-fsharp/Giraffe/blob/master/samples/IdentityApp/IdentityApp/Program.fs
let configureServices (services : IServiceCollection) =

    ////////////////////////////////////////// IN MEMORY Block ////////////////////////////////////////////////

    // Configure InMemory Db for sample application
    //services.AddDbContext<IdentityDbContext<IdentityUser>>(
    //    fun options ->
    //        options.UseInMemoryDatabase("NameOfDatabase") |> ignore
    //    ) |> ignore

    ////////////////////////////////////////// End of IN MEMORY Block ////////////////////////////////////////////////

    ///////////////////////////////////////// SQL SERVER Block ////////////////////////////////////////////////////

    ///This database is created by the CSharp dummy project
    services.AddDbContext<IdentityDbContext>(
        fun options ->
            options.UseSqlServer(
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=aspnet-EFIdentityDummyProject-5EB33349-BFC7-4D68-9488-5B635B1057A9;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
            ) |> ignore
        ) |> ignore

    ///////////////////////////////////////// End of SQL SERVER Block ////////////////////////////////////////////////////

    ///You can use either one of the following services.AddIdentity blocks. bot should work just fine

    //Block 1; example taken from https://github.com/giraffe-fsharp/Giraffe/blob/master/samples/IdentityApp/IdentityApp/Program.fs
    // Register Identity Dependencies
    //services.AddIdentity<IdentityUser, IdentityRole>(
    //    fun options ->
    //        // Password settings
    //        options.Password.RequireDigit   <- true
    //        options.Password.RequiredLength <- 8
    //        options.Password.RequireNonAlphanumeric <- false
    //        options.Password.RequireUppercase <- true
    //        options.Password.RequireLowercase <- false

    //        // Lockout settings
    //        options.Lockout.DefaultLockoutTimeSpan  <- TimeSpan.FromMinutes 30.0
    //        options.Lockout.MaxFailedAccessAttempts <- 10

    //        // User settings
    //        options.User.RequireUniqueEmail <- true
    //    )
    //    .AddEntityFrameworkStores<IdentityDbContext>()
    //    .AddDefaultTokenProviders()
    //    |> ignore

    //Block 2; example taken from C# Asp.Net Core Webapplication
    services.AddDefaultIdentity<IdentityUser>(
        fun options ->
            options.User.RequireUniqueEmail <- true
        )
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders()
        |> ignore


    // Configure app cookie
    services.ConfigureApplicationCookie(
        fun options ->
            options.ExpireTimeSpan <- TimeSpan.FromDays 150.0
        ) |> ignore

    // Enable CORS
    services.AddCors() |> ignore

    // Configure Giraffe dependencies
    services.AddGiraffe()

/// necessary to authenticate when logged in!
let configureApp (app : IApplicationBuilder) =
    app.UseAuthentication()

// exmp http://localhost:8080/api/ICounterApi/initialCounter
// exmp http://localhost:8080/api/ISecuredApi/securedCounter
//https://zaid-ajaj.github.io/Fable.Remoting/
let webApp =

    let userApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromValue counterApi
        |> Remoting.withDiagnosticsLogger (printfn "%s")
        |> Remoting.buildHttpHandler

    let dotnetServiceApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromContext dotnetApi
        |> Remoting.withDiagnosticsLogger (printfn "%s")
        |> Remoting.buildHttpHandler

    let dotnetSecureApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromContext dotnetSecureApi
        |> Remoting.withDiagnosticsLogger (printfn "%s")
        |> Remoting.buildHttpHandler

    let mustBeLoggedIn : HttpHandler =
        requiresAuthentication (redirectTo false "/")

    router {
        not_found_handler (setStatusCode 404 >=> text "Not Found")
        //forward "/mygtest" myPaths
        forward "" dotnetServiceApi
        forward "" userApi
        forward "" (mustBeLoggedIn >=> dotnetSecureApi)
    }

//https://saturnframework.org/docs/api/application/
let app =
    application {
    app_config configureApp
    service_config configureServices
    use_router webApp
    url ("http://0.0.0.0:" + port.ToString() + "/")
    memory_cache
    use_static publicPath
    use_iis
    use_gzip
    }

run app