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
open System.Security.Claims

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
        match user.Username,user.Password with
        | "","" -> LoginFail "Password and Username are empty"
        | "",_ -> LoginFail "Username is empty"
        | _,"" -> LoginFail "Password is empty"
        | _,_ ->
            task {
                let signInManager = context.GetService<SignInManager<IdentityUser>>()
                let! result = signInManager.PasswordSignInAsync(user.Username, user.Password, true, false)
                match result.Succeeded with
                | true ->
                   return LoginSuccess (result.ToString())
                | false -> return LoginFail (result.ToString())
            } |> fun x -> x.Result

    let dotnetDeleteAccount (loginModel:LoginModel) (context: HttpContext) =
        if context.User.HasClaim (ClaimTypes.Name,loginModel.Username) 
        then
            task {
                let signInManager = context.GetService<SignInManager<IdentityUser>>()
                let! result = signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, true, false)
                match result.Succeeded with
                | true ->
                    let userManager = context.GetService<UserManager<IdentityUser>>()
                    let! user = userManager.GetUserAsync context.User
                    let! deleteResult = userManager.DeleteAsync user
                    match deleteResult.Succeeded with
                    | true -> return DeleteSuccess (deleteResult.ToString())
                    | false -> return DeleteFail (deleteResult.Errors |> Seq.map (fun  x-> x.Code + " - " + x.Description) |> String.concat ". ")
                | false -> return DeleteFail "Error 401 Access Denied"
            } |> fun x -> x.Result
        else DeleteFail "Error 401 Access Denied"

    let adminDeleteAccount (loginModel:LoginModel) (userInput:User) (context: HttpContext) =
        if context.User.HasClaim (ClaimTypes.Role,"Developer") || context.User.HasClaim (ClaimTypes.Role,"Admin") || context.User.HasClaim (ClaimTypes.Role,"Usermanager")
        then
            task {
                let signInManager = context.GetService<SignInManager<IdentityUser>>()
                let! result = signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, true, false)
                match result.Succeeded with
                | true ->
                    let userManager = context.GetService<UserManager<IdentityUser>>()
                    let! findByName = userManager.FindByNameAsync(userInput.Username)
                    let! findByEmail = userManager.FindByEmailAsync(userInput.Email)
                    let user = if findByName = findByEmail then findByName else failwith "Username and Email do not correlate"
                    let! deleteResult = userManager.DeleteAsync user
                    match deleteResult.Succeeded with
                    | true -> return DeleteSuccess (deleteResult.ToString())
                    | false -> return DeleteFail (deleteResult.Errors |> Seq.map (fun  x-> x.Code + " - " + x.Description) |> String.concat ". ")
                | false -> return DeleteFail "Error 401 Access Denied"
            } |> fun x -> x.Result
        else DeleteFail "Error 401 Access Denied"

    let dotnetGetUser (context: HttpContext) =
        task {
            let userManager = context.GetService<UserManager<IdentityUser>>()
            let! user = userManager.GetUserAsync context.User
            let! getRole= (userManager.GetClaimsAsync(user))
            let role = getRole |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> (snd >> Shared.AuxFunctions.stringToRoles)
            return { Username = user.UserName; Email = user.Email; Role = role }
        } |> fun x -> x.Result

    let dotnetGetAllUsers (context: HttpContext) =
        task {
            let userManager = context.GetService<UserManager<IdentityUser>>()
            let! userList = userManager.Users.ToArrayAsync()
            let createUser =
                userList
                |> Array.map (
                    fun x ->
                        let getRole = (userManager.GetClaimsAsync(x))
                        let role = getRole.Result |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> (snd >> Shared.AuxFunctions.stringToRoles)
                        {
                            Username = x.UserName;
                            Email = x.Email;
                            Role = role
                        }
                    )
            return createUser
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
                let! addingClaims = userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "User"))
                match addingClaims.Succeeded with
                | false -> return (RegisterFail (showErrors result.Errors))
                | true  -> let signInManager = context.GetService<SignInManager<IdentityUser>>()
                           do! signInManager.SignInAsync(user, true)
                           return RegisterSuccess "Registration Successful"
        } |> fun x -> x.Result

    let adminUserRegistration (registerModel:RegisterModel) (role:ActiveUserRoles) (context: HttpContext) =
        if role = Developer || role = Guest
        then RegisterFail "Error 401 Access Denied"
        else
            task {
                let  user        = IdentityUser(UserName = registerModel.Username, Email = registerModel.Email)
                let  userManager = context.GetService<UserManager<IdentityUser>>()
                let! result      = userManager.CreateAsync(user, registerModel.Password)
                match result.Succeeded with
                | false -> return (RegisterFail (showErrors result.Errors))
                | true  ->
                    let! addingClaims = userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, string role))
                    match addingClaims.Succeeded with
                    | false -> return (RegisterFail (showErrors result.Errors))
                    | true  -> return RegisterSuccess "Registration Successful"
            } |> fun x -> x.Result

let nice() =
    { Value = 69 }

let dotnetApi (context: HttpContext) = {
    dotnetLogin = fun (loginModel) -> async { return (DotnetModule.dotnetLogin loginModel context) }
    dotnetRegister = fun (registerModel) -> async { return (DotnetModule.dotnetRegistration registerModel context) }
}

let dotnetSecureApi (context: HttpContext) = {
    getUserCounter = fun () -> async { return nice() }
    dotnetGetUser = fun () -> async { return (DotnetModule.dotnetGetUser context)}
    dotnetUserLogOut = fun () -> async { return (DotnetModule.dotnetUserLogOut context) }
    dotnetDeleteUserAccount = fun loginModel -> async {return (DotnetModule.dotnetDeleteAccount loginModel context)}
}

let counterApi = {
    initialCounter = fun () -> async {return { Value = 42 }}
}

let adminSecureApi (context: HttpContext) = {
    dotnetGetAllUsers = fun () -> async { return DotnetModule.dotnetGetAllUsers context}
    adminRegisterUser = fun (registerModel,userRole) -> async { return DotnetModule.adminUserRegistration registerModel userRole context}
    adminDeleteAccount = fun (loginModel,user) -> async { return DotnetModule.adminDeleteAccount loginModel user context }
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
            options.Password.RequireDigit <- false
            options.Password.RequiredLength <- 4
            options.Password.RequiredUniqueChars <- 2
            options.Password.RequireLowercase <- false
            options.Password.RequireNonAlphanumeric <- false
            options.Password.RequireUppercase <- false
        )
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders()
        //.AddRoleManager<IdentityRole>()
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

    let adminSecureApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromContext adminSecureApi
        |> Remoting.withDiagnosticsLogger (printfn "%s")
        |> Remoting.buildHttpHandler

    let mustBeLoggedIn : HttpHandler =
        requiresAuthentication (redirectTo false "/")

    let mustBeDeveloper : HttpHandler =
        authorizeUser (fun u -> u.HasClaim (ClaimTypes.Name, "Kevin") || u.HasClaim (ClaimTypes.Role, "Developer")) (setStatusCode 401 >=> text "Access Denied")

    router {
        not_found_handler (setStatusCode 404 >=> text "Not Found")
        //forward "/mygtest" myPaths
        forward "" dotnetServiceApi
        forward "" userApi
        forward "" (mustBeLoggedIn >=> dotnetSecureApi)
        forward "" (mustBeLoggedIn >=> mustBeDeveloper >=> adminSecureApi)
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