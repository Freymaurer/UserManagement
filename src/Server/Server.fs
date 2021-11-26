module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Giraffe
open Saturn

open System
open System.Threading.Tasks
open System.Net.Http
open System.Net.Http.Headers
open System.Security.Claims
open System.Text.Json
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.EntityFrameworkCore

open Shared
open StaticStrings

type Storage() =
    let todos = ResizeArray<_>()

    member __.GetTodos() = List.ofSeq todos

    member __.AddTodo(todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

let storage = Storage()

storage.AddTodo(Todo.create "Create new SAFE project")
|> ignore

storage.AddTodo(Todo.create "Write your app")
|> ignore

storage.AddTodo(Todo.create "Ship it !!!")
|> ignore

let todosApi = {
    getTodos = fun () -> async { return storage.GetTodos() }
    addTodo = fun todo -> async {
        match storage.AddTodo todo with
        | Ok () -> return todo
        | Error e -> return failwith e
    }
}

let identityApi (ctx: HttpContext) : IIdentityApi = {
    login = fun loginInfo -> async {return UserIdentity.login loginInfo ctx}
    register = fun signupInfo -> async { return UserIdentity.signup signupInfo ctx }
    getNumTest = fun () -> async { return 42 }
}

let userApi (ctx: HttpContext) : IUserApi = {
    getActiveUser       = fun ()    -> async { return UserIdentity.getActiveUser ctx }
    updateUserProfile   = fun user  -> async { return UserIdentity.updateUserProfile user ctx }
    logout              = fun ()    -> async { return UserIdentity.logout ctx }
    getHelloUser        = fun ()    -> async {
        let user = ctx.User.Identity.Name
        let msg = $"Hello {user}! I wish you the most wonderful day!"
        return msg
    }
}

let adminApi (ctx: HttpContext) : IAdminApi = {
    getHelloAdmin = fun () -> async {
        let user = ctx.User.Identity.Name
        let msg = $"Hello {user}! It is an honor to be in the company of such a great admin!"
        return msg
    }
}

let errorHandler (ex:exn) (routeInfo:RouteInfo<HttpContext>) =
    let msg = sprintf "[SERVER ERROR]: %A @%s." ex.Message routeInfo.path
    Propagate msg

let webApp =
    let todoApi =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromValue todosApi
        |> Remoting.withDiagnosticsLogger(printfn "%A")
        |> Remoting.withErrorHandler errorHandler
        |> Remoting.buildHttpHandler

    let identityApi =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromContext identityApi
        |> Remoting.withDiagnosticsLogger(printfn "%A")
        |> Remoting.withErrorHandler errorHandler
        |> Remoting.buildHttpHandler

    let userApi =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromContext userApi
        |> Remoting.withDiagnosticsLogger(printfn "%A")
        |> Remoting.withErrorHandler errorHandler
        |> Remoting.buildHttpHandler

    let adminApi =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromContext adminApi
        |> Remoting.withDiagnosticsLogger(printfn "%A")
        |> Remoting.withErrorHandler errorHandler
        |> Remoting.buildHttpHandler

    let parsableError errorMsg =
        sprintf """{"error":"%s", "ignored" : false, "handled" : false}""" errorMsg

    let mustBeLoggedIn : HttpHandler =
        requiresAuthentication (
            setStatusCode 401 >=> json (parsableError "Access Denied, not logged in.")
        )

    let mustBeAdmin : HttpHandler =
        authorizeUser
            ( fun u ->
                u.HasClaim (ClaimTypes.Role, string IdentityTypes.Developer)
                || u.HasClaim (ClaimTypes.Role, string IdentityTypes.Admin)
            )
            (setStatusCode 401
                >=> json (parsableError "Access Denied, not an admin.")
            )

    // The exact order of the following routes is important to guarantee correct auth.
    // Appearently, the "mustBeLoggedIn >=> mustBeAdmin >=>" Syntax will provoke "access denied results" even if the correct
    // url is not hit! This is most likely due to "forward "" ", which is at the moment necessary for fable.remoting.
    router {
        get "/test/test1" (htmlString "<h1>Hi this is test response 1</h1>")
        // urls for challenge against oauth login
        forward StaticStrings.OAuthPaths.GoogleOAuth OAuth.googleAuth
        forward StaticStrings.OAuthPaths.GithubOAuth OAuth.gitHubAuth
        forward StaticStrings.OAuthPaths.OrcidOAuth OAuth.orcidAuth
        // oauth callback: creates useraccount and external user login for oauth user
        forward OAuthPaths.ExternalLoginCallback (OAuth.externalLoginCallback >=> redirectTo false SameSiteUrls.PageUrl)
        // extra log out url; not necessary
        forward SameSiteUrls.logoutUrl (signOut "Cookies")
        forward "" todoApi
        forward "" identityApi
        forward "" (mustBeLoggedIn >=> userApi)
        forward "" (mustBeLoggedIn >=> mustBeAdmin >=> adminApi)
        not_found_handler (
            setStatusCode 404 >=> text "Page not found."
        )
    }


/// Client ids and Client secrets
let testGoogleId = "84896855857-3sq30njitdkb44mme3ksvh3vj829ei57.apps.googleusercontent.com"
let testGoogleSecret = "jZWx6xzNKmMjUjQT7GZc7JJV"

let testGithubId = "7aa4b09af402a61e6dbc"
let testGithubSecret = "84260ae6515a87e101eb1e8a51d71bb91d980beb"

let testOrcidId = "APP-P5S1R3ZBQWZL1NN9"
let testOrcidSecret = "bc292a25-5492-45b0-9355-9745c545729a"

let dockerdb = @"Data Source=localhost;Initial Catalog=UserManagement;User ID=sa;Password=Testing#11;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"

///https://github.com/giraffe-fsharp/Giraffe/blob/master/samples/IdentityApp/IdentityApp/Program.fs
let configureServices (services : IServiceCollection) =

    ///////////////////////////////////////// SQL SERVER Block ////////////////////////////////////////////////////

    ///This database is created by the CSharp dummy project
    services.AddDbContext<IdentityDbContext>(
        fun options ->
            options.UseSqlServer(
                // this db context can either be a connection string to a local db, as in this case (see readme on how to create this) or a connection string to an external sql server.
                dockerdb
            ) |> ignore
        ) |> ignore
    //
    services.AddDefaultIdentity<IdentityUser>(
        fun options ->

            /// https://docs.microsoft.com/de-de/aspnet/core/security/authentication/identity?view=aspnetcore-3.1&tabs=visual-studio
            /// https://docs.microsoft.com/de-de/aspnet/core/security/authentication/accconfirm?view=aspnetcore-3.1&tabs=visual-studio#prevent-login-at-registration
            //options.SignIn.RequireConfirmedAccount <- true

            // Password settings.
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

    services.Configure<IdentityOptions>(
        fun (options:IdentityOptions) ->

            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan <- TimeSpan.FromMinutes(5.)
            options.Lockout.MaxFailedAccessAttempts <- 5
            options.Lockout.AllowedForNewUsers <- true

            options.User.AllowedUserNameCharacters <- "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"
        ) |> ignore

    /// add OAuth authentications; always add callback link to create user accounts!
    services.AddAuthentication(
        fun options ->
        //    options.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
        //    options.DefaultSignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            //adds authentification for normal Asp.net core identity model
            options.DefaultAuthenticateScheme <- IdentityConstants.ApplicationScheme
            options.DefaultSignInScheme <- IdentityConstants.ApplicationScheme
        )
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
            fun options ->
                options.LogoutPath <- PathString SameSiteUrls.logoutUrl
                //options.LoginPath <- PathString "/api/Account/Login";
                //options.AccessDeniedPath <- PathString "/api/Account/AccessDenied";
        )
        //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
        //    fun options ->
        //        options
        //)
        // source: https://www.eelcomulder.nl/2018/06/12/secure-your-giraffe-application-with-an-oauth-provider/
        // https://developers.google.com/identity/protocols/OAuth2
        .AddGoogle("Google",
            fun options ->
                options.ClientId <- testGoogleId
                options.ClientSecret <- testGoogleSecret

                let ev = options.Events

                ev.OnTicketReceived <-
                    fun ctx ->
                        let tsk = task {
                            ctx.ReturnUri <- OAuthPaths.ExternalLoginCallback
                        }
                        Task.Factory.StartNew(fun () -> tsk.Result)
        )
        // source: https://github.com/SaturnFramework/Saturn/blob/master/src/Saturn.Extensions.Authorization/OAuth.fs
        // https://github.com/settings/developers
        .AddOAuth("GitHub",
            fun (options:OAuth.OAuthOptions) ->
                options.ClientId <- testGithubId
                options.ClientSecret <- testGithubSecret
                options.CallbackPath <- PathString("/signin-github")

                options.AuthorizationEndpoint <- "https://github.com/login/oauth/authorize"
                options.TokenEndpoint <- "https://github.com/login/oauth/access_token"
                options.UserInformationEndpoint <- "https://api.github.com/user"

                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name")
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id")
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email")
                options.ClaimActions.MapJsonKey(ClaimTypes.Locality, "location")

                let ev = options.Events
                ev.OnTicketReceived <-
                    fun ctx ->
                        let tsk = task {
                            ctx.ReturnUri <- OAuthPaths.ExternalLoginCallback
                        }
                        Task.Factory.StartNew(fun () -> tsk.Result)
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
        /// source: https://members.orcid.org/api/tutorial/get-orcid-id
        .AddOAuth("Orcid",
            fun (options:OAuth.OAuthOptions) ->
                options.ClientId <- testOrcidId
                options.ClientSecret <- testOrcidSecret
                options.CallbackPath <- PathString("/signin-orcid")
                options.Scope.Add "/authenticate"//"openid" ///"/read-limited" needs member api - 6.5k annual

                options.AuthorizationEndpoint <- "https://orcid.org/oauth/authorize"
                options.TokenEndpoint <- "https://orcid.org/oauth/token"
                /////the outcommented code needs the "openid" scope
                //options.UserInformationEndpoint <- "https://orcid.org/oauth/userinfo"

                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name")
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier,"orcid")

                let ev = options.Events
                ev.OnTicketReceived <-
                    fun ctx ->
                        let tsk = task {
                            ctx.ReturnUri <- OAuthPaths.ExternalLoginCallback
                        }
                        Task.Factory.StartNew(fun () -> tsk.Result)
                ev.OnCreatingTicket <-
                    fun ctx ->
                        let tsk = task {
                        /////the outcommented code needs the "openid" scope
                            //let req = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint)
                            //req.Headers.Accept.Add(MediaTypeWithQualityHeaderValue("application/json"))
                            //req.Headers.Authorization <- AuthenticationHeaderValue("Bearer", ctx.AccessToken)
                            //let! (response : HttpResponseMessage) = ctx.Backchannel.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ctx.HttpContext.RequestAborted)
                            //response.EnsureSuccessStatusCode () |> ignore
                            //let! cnt = response.Content.ReadAsStringAsync()
                            //let user = JsonDocument.Parse cnt |> fun x -> x.RootElement
                            //ctx.RunClaimActions user
                            let claims = ctx.TokenResponse.Response.RootElement
                            ctx.RunClaimActions claims
                        }
                        Task.Factory.StartNew(fun () -> tsk.Result)
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
        url "https://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
        force_ssl
    }

run app
