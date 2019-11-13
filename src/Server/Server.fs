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
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication
open FSharp.Control.Tasks.V2.ContextInsensitive
open System.Net.Http
open System.Net.Http.Headers
open System.Text.Json
open System.Threading.Tasks

open AspNetCoreIdentity


let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let nice() =
    { Value = 69 }

let userHandler (ctx: HttpContext) =
    let nameClaim = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Name)
    let emailClaim = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Email)
    let testing =
        ctx.User.Claims
        |> Seq.map (fun x -> sprintf "%s ->> %s <br><br>" x.Type x.Value)
        |> String.concat ""
    if ctx.User.Identity.IsAuthenticated then
        testing
        //[|nameClaim.Value;emailClaim.Value;nameClaim.Issuer|]
        //|> String.concat "; "
    else
        "Not logged in"

module OAuth =

    let private challenge (scheme : string) (redirectUri : string) : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                do! ctx.ChallengeAsync(
                        scheme,
                        AuthenticationProperties(RedirectUri = redirectUri))
                return! next ctx
            }

    let gitHubAuth =
        challenge "GitHub" "http://localhost:8080/"

    let googleAuth =
        challenge "Google" "http://localhost:8080/"


let dotnetApi (context: HttpContext) = {
    dotnetLogin = fun (loginModel) -> async { return (dotnetLogin loginModel context) }
    dotnetRegister = fun (registerModel) -> async { return (dotnetRegistration registerModel context) }
    getContextClaims = fun () -> async { return (userHandler context)}
    initialCounter = fun () -> async {return { Value = 42 }}
}

let dotnetSecureApi (context: HttpContext) = {
    getUserCounter = fun () -> async { return nice() }
    dotnetGetUser = fun () -> async { return (dotnetGetUser context)}
    dotnetUserLogOut = fun () -> async { return dotnetUserLogOut context }
    dotnetDeleteUserAccount = fun loginModel -> async {return (dotnetDeleteAccount loginModel context)}
    dotnetChangeUserParameters = fun (loginModel,userParam,input) -> async { return dotnetChangeUserParams loginModel userParam input context }
}


let adminSecureApi (context: HttpContext) = {
    dotnetGetAllUsers = fun () -> async { return dotnetGetAllUsers context}
    adminRegisterUser = fun (registerModel,userRole) -> async { return adminUserRegistration registerModel userRole context}
    adminDeleteAccount = fun (loginModel,user) -> async { return adminDeleteAccount loginModel user context }
    adminChangeUserParameters = fun (loginModel,userInput,userParam,input) -> async { return adminChangeUserParams loginModel userInput userParam input context }
}



// exmp http://localhost:8080/api/ICounterApi/initialCounter
// exmp http://localhost:8080/api/ISecuredApi/securedCounter
//https://zaid-ajaj.github.io/Fable.Remoting/
let webApp =

    let userApi =
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
        requiresAuthentication (redirectTo false "/")

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

    router {
        not_found_handler (setStatusCode 404 >=> text "Not Found")
        forward "/api/testing" (text "hello")
        forward "/api/google-auth" OAuth.googleAuth
        forward "/api/github-auth" OAuth.gitHubAuth
        forward "/api/logout" (signOut "Cookies")
        forward "" userApi
        forward "" (mustBeLoggedIn >=> dotnetSecureApi)
        forward "" (mustBeLoggedIn >=> mustBeUserManager >=> adminSecureApi)
    }

let testGoogleId = "84896855857-3sq30njitdkb44mme3ksvh3vj829ei57.apps.googleusercontent.com"
let testGoogleSecret = "jZWx6xzNKmMjUjQT7GZc7JJV"

let testGithubId = "7aa4b09af402a61e6dbc"
let testGithubSecret = "84260ae6515a87e101eb1e8a51d71bb91d980beb"

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
            // this kill identity log in, but is necessary for oauth login. My guess, one works via token login and one via cookie log in
            options.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            options.DefaultSignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- "Google"
        )
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
            fun options -> 
                options.LogoutPath <- PathString "/api/logout-g"
            )

        // source: https://www.eelcomulder.nl/2018/06/12/secure-your-giraffe-application-with-an-oauth-provider/
        // https://developers.google.com/identity/protocols/OAuth2
        .AddGoogle("Google",
            fun options ->
                options.ClientId <- testGoogleId
                options.ClientSecret <- testGoogleSecret
        )
        // source: https://github.com/SaturnFramework/Saturn/blob/master/src/Saturn.Extensions.Authorization/OAuth.fs
        // https://github.com/settings/developers
        .AddOAuth("GitHub",
            fun (options:OAuth.OAuthOptions) ->
                options.ClientId <- testGithubId
                options.ClientSecret <- testGithubSecret
                options.CallbackPath <- new PathString("/signin-github") 
 
                options.AuthorizationEndpoint <- "https://github.com/login/oauth/authorize"
                options.TokenEndpoint <- "https://github.com/login/oauth/access_token"
                options.UserInformationEndpoint <- "https://api.github.com/user"

                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name")
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id")
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email")
                options.ClaimActions.MapJsonKey(ClaimTypes.Locality, "location")

                let ev = options.Events
                ev.OnCreatingTicket <-
                    fun ctx ->
                      let tsk = task {
                        let req = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint)
                        req.Headers.Accept.Add(MediaTypeWithQualityHeaderValue("application/json"))
                        req.Headers.Authorization <- AuthenticationHeaderValue("Bearer", ctx.AccessToken)
                        let! (response : HttpResponseMessage) = ctx.Backchannel.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ctx.HttpContext.RequestAborted)
                        response.EnsureSuccessStatusCode () |> ignore
                        let! cnt = response.Content.ReadAsStringAsync()
                        let user = JsonDocument.Parse cnt |> fun x -> x.RootElement
                        ctx.RunClaimActions user
                      }
                      Task.Factory.StartNew(fun () -> tsk.Result)
        )
        |> ignore
    //adds authentification for normal Asp.net core identity model
    services.AddAuthentication(
        fun options ->
            options.DefaultAuthenticateScheme <- IdentityConstants.ApplicationScheme
            options.DefaultSignInScheme <- IdentityConstants.ApplicationScheme
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