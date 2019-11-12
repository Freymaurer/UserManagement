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
open System.Security.Claims

open AspNetCoreIdentity
open FSharp.Control.Tasks
open Microsoft.Extensions.Configuration
open System.Security.Cryptography.X509Certificates
open Microsoft.AspNetCore.Server.Kestrel.Core
open System.Net

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let nice() =
    { Value = 69 }

let dotnetApi (context: HttpContext) = {
    dotnetLogin = fun (loginModel) -> async { return (dotnetLogin loginModel context) }
    dotnetRegister = fun (registerModel) -> async { return (dotnetRegistration registerModel context) }
    githubSignIn = fun () -> async { return "nice"}
}

let dotnetSecureApi (context: HttpContext) = {
    getUserCounter = fun () -> async { return nice() }
    dotnetGetUser = fun () -> async { return (dotnetGetUser context)}
    dotnetUserLogOut = fun () -> async { return dotnetUserLogOut context }
    dotnetDeleteUserAccount = fun loginModel -> async {return (dotnetDeleteAccount loginModel context)}
    dotnetChangeUserParameters = fun (loginModel,userParam,input) -> async { return dotnetChangeUserParams loginModel userParam input context }
}

let counterApi = {
    initialCounter = fun () -> async {return { Value = 42 }}
}

let adminSecureApi (context: HttpContext) = {
    dotnetGetAllUsers = fun () -> async { return dotnetGetAllUsers context}
    adminRegisterUser = fun (registerModel,userRole) -> async { return adminUserRegistration registerModel userRole context}
    adminDeleteAccount = fun (loginModel,user) -> async { return adminDeleteAccount loginModel user context }
    adminChangeUserParameters = fun (loginModel,userInput,userParam,input) -> async { return adminChangeUserParams loginModel userInput userParam input context }
}


open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication
open FSharp.Control.Tasks.V2.ContextInsensitive

//let googleAuth = challengeG "Google" "/api/IDotnetSecureApi/dotnetGetUser"

let googleAuth =
    let challenge (scheme : string) (redirectUri : string) : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                do! ctx.ChallengeAsync(
                        scheme,
                        AuthenticationProperties(RedirectUri = redirectUri))
                return! next ctx
            }
    challenge "Google" "http://localhost:8080/"


let userHandler (ctx: HttpContext) =
    let nameClaim = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Name)
    let emailClaim = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Email)
    if ctx.User.Identity.IsAuthenticated then 
        [|nameClaim.Value;emailClaim.Value;nameClaim.Issuer|]
        |> String.concat "; "
    else
        "Not logged in"

let oauthApi (context: HttpContext) = {
    getUserFromGoogle = fun () -> async { return (userHandler context)}
}


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

    let adminSecureApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromContext adminSecureApi
        |> Remoting.withDiagnosticsLogger (printfn "%s")
        |> Remoting.buildHttpHandler


    let mustBeLoggedIn : HttpHandler =
        requiresAuthentication (text "normal authentication failed")//(redirectTo false "/")

    let mustBeUserManager : HttpHandler =
        authorizeUser (
            fun u ->
                u.HasClaim (ClaimTypes.Name, "Kevin")
                || u.HasClaim (ClaimTypes.Role, "Developer")
                || u.HasClaim (ClaimTypes.Role, "Admin")
                || u.HasClaim (ClaimTypes.Role, "UserManager")
        ) (
            setStatusCode 401 >=> text "Access Denied"
        )

    let oAuthApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromContext oauthApi
        |> Remoting.withDiagnosticsLogger (printfn "%s")
        |> Remoting.buildHttpHandler

    router {
        not_found_handler (setStatusCode 404 >=> text "Not Found")
        forward "/api/testing" (text "hello")
        forward "/api/google-auth" googleAuth
        forward "/api/logout-g" (signOut "Cookies" >=> (redirectTo false "http://localhost:8080/") )
        forward "" dotnetServiceApi
        forward "" userApi
        forward "" oAuthApi
        forward "" (mustBeLoggedIn >=> dotnetSecureApi)
        forward "" (mustBeLoggedIn >=> mustBeUserManager >=> adminSecureApi)
    }

let testClientId = "84896855857-3sq30njitdkb44mme3ksvh3vj829ei57.apps.googleusercontent.com"
let testClientSecret = "jZWx6xzNKmMjUjQT7GZc7JJV"

///https://github.com/giraffe-fsharp/Giraffe/blob/master/samples/IdentityApp/IdentityApp/Program.fs
let configureServices (services : IServiceCollection) =

    ///////////////////////////////////////// SQL SERVER Block ////////////////////////////////////////////////////

    ///This database is created by the CSharp dummy project
    services.AddDbContext<IdentityDbContext>(
        fun options ->
            options.UseSqlServer(
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=aspnet-EFIdentityDummyProject-5EB33349-BFC7-4D68-9488-5B635B1057A9;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
            ) |> ignore
        ) |> ignore

    //Block 2; example taken from C# Asp.Net Core Webapplication
    services.AddDefaultIdentity<IdentityUser>(
        fun options ->
            options.User.RequireUniqueEmail <- true
            options.Password.RequireDigit <- false
            options.Password.RequiredLength <- 4
            options.Password.RequiredUniqueChars <- 2
            options.Password.RequireLowercase <- false
            options.Password.RequireNonAlphanumeric <- false
            options.Password.RequireUppercase <- false
        )
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders()
        |> ignore

    services.AddAuthentication(
        fun options ->
            ///// this kill identity log in, but is necessary for oauth login. My guess, one works via token login and one via cookie log in
            options.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            options.DefaultSignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- "Google"
        )
        .AddCookie(
            fun options ->
                options.LogoutPath <- PathString "/api/logout-g"
            )
        .AddGoogle("Google",
            fun options ->
                options.ClientId <- testClientId
                options.ClientSecret <- testClientSecret
        )
        |> ignore
    services.AddAuthentication(
        fun options ->
            options.DefaultAuthenticateScheme <- "Identity.Application"
            options.DefaultSignInScheme <- "Identity.Application"
        )
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

//https://saturnframework.org/docs/api/application/
let app =
    application {
    app_config (fun a -> a.UseAuthentication().UseAuthorization() )
    service_config configureServices
    use_router webApp
    url ("http://0.0.0.0:" + port.ToString() + "/")
    memory_cache
    use_static publicPath
    use_iis
    use_gzip
    }

run app