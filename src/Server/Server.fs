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
open Microsoft.AspNetCore.Mvc

module OAuthSigninPaths =

    let googleOAuth = "/api/google-auth"
    let githubOAuth = "/api/github-auth"
    let orcidOAuth = "/api/orcid-auth"

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let nice() =
    { Value = 69 }

let userHandler (ctx: HttpContext) =
    //let nameClaim = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Name)
    //let emailClaim = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Email)
    let testing =
        ctx.User.Claims
        |> Seq.map (fun x -> sprintf "%s ->> %s <br><br>" x.Type x.Value)
        |> String.concat ""
    if ctx.User.Identity.IsAuthenticated then
        testing
    else
        "Not logged in"

let checkAccountOrigin (ctx: HttpContext) =
    let checkOAuth = ctx.User.HasClaim (fun c -> c.Type = CustomClaims.LoginMethod && c.Value = LoginMethods.LocalAuthority)
    if checkOAuth = true
    then LoginMethods.LocalAuthority
    else
        ctx.User.Claims
        |> Seq.map (fun x -> x.Issuer)
        |> Seq.filter (fun x -> x <> LoginMethods.LocalAuthority)
        |> Seq.groupBy (fun x -> x)
        |> fun x -> if Seq.length x > 1 then failwith "Unknown Claims issuer!" else x |> (Seq.head >> fst)

module OAuth =

    let private challenge (scheme : string) (redirectUri : string) : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let signinManager = ctx.GetService<SignInManager<IdentityUser>>()
                let properties = signinManager.ConfigureExternalAuthenticationProperties(scheme, redirectUri)
                do! ctx.ChallengeAsync(
                        scheme,
                        properties
                    )
                return! next ctx
            }

    let gitHubAuth =
        challenge "GitHub" "http://localhost:8080"

    let googleAuth =
        challenge "Google" "http://localhost:8080"

    let orcidAuth =
        challenge "Orcid" "http://localhost:8080"

    open GiraffeViewEngine

    type Message = {
        Text    :   string
    }

    let oauthTesting (next : HttpFunc) (ctx : HttpContext) =
        let infoMod = {Text = "Testing"}
        let layout (content: XmlNode list) =
            html [] [
                head [] [
                    title []  [ encodedText "Giraffe on Asp.Net Core with OAuth" ]
                    link [ _rel  "stylesheet"
                           _type "text/css"
                           _href "/main.css" ]
                ]
                body [] content
            ]
        let partial () =
            h1 [] [ encodedText "Hello Giraffe" ]
        let index (model : Message) =
            [
                partial()
                p [] [ encodedText model.Text ]
            ] |> layout
        htmlView (index infoMod) next ctx

let externalLoginCallback : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let signinManager = ctx.GetService<SignInManager<IdentityUser>>()
            let uniqueIdent = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.NameIdentifier) |> fun x -> x.Value
            let issuer = checkAccountOrigin ctx
            let! signInResult = signinManager.ExternalLoginSignInAsync(issuer,uniqueIdent,true)
            match signInResult.Succeeded with
            |false ->
                printfn "%s" uniqueIdent
                printfn "1"
                let emailClaim =
                    if ctx.User.HasClaim (fun c -> c.Type = ClaimTypes.Email)
                    then ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Email) |> fun x -> x.Value
                    else issuer + "@" + uniqueIdent
                printfn "%s" emailClaim
                printfn "2"
                printfn "3"
                let user = new IdentityUser(userName = emailClaim, Email = emailClaim)
                let userManager = ctx.GetService<UserManager<IdentityUser>>()
                let! createResult = userManager.CreateAsync(user)
                printfn "4"
                match createResult.Succeeded with
                        | true ->
                            printfn "5"
                            let info = UserLoginInfo(issuer,uniqueIdent,issuer)
                            let! addLoginResult = userManager.AddLoginAsync(user,info)
                            printfn "6"
                            match addLoginResult.Succeeded with
                            | true ->
                                printfn "7"
                                let! addClaims =
                                    userManager.AddClaimsAsync(user,[
                                            Claim(
                                                ClaimTypes.Role,"Guest",ClaimValueTypes.String,LoginMethods.LocalAuthority
                                            );
                                            Claim(
                                                CustomClaims.LoginMethod,issuer,ClaimValueTypes.String,LoginMethods.LocalAuthority
                                            )
                                            Claim(
                                                CustomClaims.IsUsernameSet,"false",ClaimValueTypes.Boolean,LoginMethods.LocalAuthority
                                            )
                                    ])
                                printfn "8"
                                let! signinResult = signinManager.SignInAsync(user,true)
                                printfn "9"
                                return! next ctx
                            | false -> return failwith "Testing 2 error"
                        | false -> return failwith "Testing 1 error"
            | true ->
                return! next ctx
        }

let addUsernameToExtLoginFunc (username:string) (ctx:HttpContext) = 
    task {
        let userManager = ctx.GetService<UserManager<IdentityUser>>()
        let! user = userManager.GetUserAsync(ctx.User)
        let! addUserNameResult = userManager.SetUserNameAsync(user,username)
        match addUserNameResult.Succeeded with
        | true ->
            let! changeClaimResult = userManager.ReplaceClaimAsync(user, Claim(CustomClaims.IsUsernameSet,"false"), Claim(CustomClaims.IsUsernameSet,"true"))
            match changeClaimResult.Succeeded with
            | true ->
                let signinManager = ctx.GetService<SignInManager<IdentityUser>>()
                let! signout = signinManager.SignOutAsync()
                let! signin = signinManager.SignInAsync(user,true)
                return "Succeded"
            | false ->
                return "Failed 2"
        | false ->
            return "Failed"
    } |> fun x -> x.Result

let testExtLogin (ctx:HttpContext) =
    task {
        let signinManager = ctx.GetService<SignInManager<IdentityUser>>()
        let! signinResult = signinManager.ExternalLoginSignInAsync("Google","104335612853700138136",true)
        printfn "%A" signinResult.Succeeded
    } |> fun x -> x.Result

let dotnetApi (context: HttpContext) = {
    dotnetLogin = fun (loginModel) -> async { return (dotnetLogin loginModel context) }
    dotnetRegister = fun (registerModel) -> async { return (dotnetRegistration registerModel context) }
    getContextClaims = fun () -> async { return (userHandler context)}
    initialCounter = fun () -> async {return { Value = 42 }}
    //testing
    externalLoginTest = fun (scheme,redirect) ->
        async {
            return
                sprintf "done %A" "!"
        }
    }

let dotnetSecureApi (context: HttpContext) = {
    getUserCounter = fun () -> async { return nice() }
    dotnetGetUser = fun () -> async { return (dotnetGetUser context)}
    dotnetUserLogOut = fun () -> async { return dotnetUserLogOut context }
    dotnetDeleteUserAccount = fun loginModel -> async {return (dotnetDeleteAccount loginModel context)}
    dotnetChangeUserParameters = fun (loginModel,userParam,input) -> async { return dotnetChangeUserParams loginModel userParam input context }
    addUsernameToExtLogin = fun (username) -> async { return addUsernameToExtLoginFunc username context }
    }

let adminSecureApi (context: HttpContext) = {
    dotnetGetAllUsers = fun () -> async { return dotnetGetAllUsers context}
    adminRegisterUser = fun (registerModel,userRole) -> async { return adminUserRegistration registerModel userRole context}
    adminDeleteAccount = fun (loginModel,user) -> async { return adminDeleteAccount loginModel user context }
    adminChangeUserParameters = fun (loginModel,userInput,userParam,input) -> async { return adminChangeUserParams loginModel userInput userParam input context }
    }

// exmp http://localhost:8080/api/ISecuredApi/securedCounter
// https://zaid-ajaj.github.io/Fable.Remoting/
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
        requiresAuthentication (
            setStatusCode 401 >=> text "Access Denied"
        )

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
        forward "/api/testing" OAuth.oauthTesting
        forward OAuthSigninPaths.googleOAuth OAuth.googleAuth
        forward OAuthSigninPaths.githubOAuth OAuth.gitHubAuth
        forward OAuthSigninPaths.orcidOAuth OAuth.orcidAuth
        forward "/api/externalLoginCallback" (externalLoginCallback >=> redirectTo false ("http://localhost:8080/"))
        forward "/api/logout" (signOut "Cookies")
        forward "" userApi
        forward "" (mustBeLoggedIn >=> dotnetSecureApi)
        forward "" (mustBeLoggedIn >=> mustBeUserManager >=> adminSecureApi)
        forward "" (setStatusCode 404 >=> text "Not Found")
    }

let testGoogleId = "84896855857-3sq30njitdkb44mme3ksvh3vj829ei57.apps.googleusercontent.com"
let testGoogleSecret = "jZWx6xzNKmMjUjQT7GZc7JJV"

let testGithubId = "7aa4b09af402a61e6dbc"
let testGithubSecret = "84260ae6515a87e101eb1e8a51d71bb91d980beb"

let testOrcidId = "APP-P5S1R3ZBQWZL1NN9"
let testOrcidSecret = "bc292a25-5492-45b0-9355-9745c545729a"

open FSharp.Control.Tasks

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

    services.Configure<IdentityOptions>(
        fun (options:IdentityOptions) ->
            options.User.AllowedUserNameCharacters <- "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"
        ) |> ignore

    services.AddAuthentication(
        fun options ->
            options.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            options.DefaultSignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            options.DefaultChallengeScheme <- "Google"
        )
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
            fun options -> 
                options.LogoutPath <- PathString "/api/logout"
            )
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
                            ctx.ReturnUri <- "/api/externalLoginCallback"
                        }
                        Task.Factory.StartNew(fun () -> tsk.Result)
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
                ev.OnTicketReceived <-
                    fun ctx ->
                        let tsk = task {
                            ctx.ReturnUri <- "/api/externalLoginCallback"
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
                options.CallbackPath <- new PathString("/signin-orcid")
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
                            ctx.ReturnUri <- "/api/externalLoginCallback"
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