module AspNetCoreIdentity

open Shared

open Giraffe

open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open FSharp.Control.Tasks
open System.Security.Claims
open System.Text

let isGoogleUser (context: HttpContext) =
    context.User.Claims
        |> Seq.exists (
            fun claim ->
                claim.Issuer = "Google"
        )

let isGitHubUser (context: HttpContext) =
    context.User.Claims 
        |> Seq.exists (
            fun claim ->
                claim.Issuer = "GitHub"
        )

let showErrors (errors : IdentityError seq) =
    errors
    |> Seq.fold (fun acc err ->
        sprintf "Code: %s, Description: %s" err.Code err.Description
        |> acc.AppendLine : StringBuilder) (StringBuilder(""))
    |> (fun x -> x.ToString())

open Microsoft.AspNetCore.Authentication

let dotnetLogin (user:LoginModel) (contextPre: HttpContext) =
    let prevSignOut (ctx: HttpContext) =
        task {
            do! ctx.SignOutAsync("Cookies")
            return ctx
        } |> fun x -> x.Result
    let context = prevSignOut contextPre
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

let dotnetChangeUserParams (loginModel:LoginModel) (userParameter:UserParameters) (input:string) (context: HttpContext) =
    if context.User.HasClaim (ClaimTypes.Name,loginModel.Username) 
    then
        task {
            let signInManager = context.GetService<SignInManager<IdentityUser>>()
            let! result = signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, true, false)
            match result.Succeeded with
            | true ->
                let userManager = context.GetService<UserManager<IdentityUser>>()
                let! user = userManager.GetUserAsync context.User
                let! updateResult =
                    match userParameter with
                    | Email -> userManager.SetEmailAsync(user,input)
                    | Username -> userManager.SetUserNameAsync(user,input)
                    | Password ->
                        let updateResult =
                            let hashPw = userManager.PasswordHasher.HashPassword(user, input)
                            user.PasswordHash <- hashPw
                        userManager.UpdateAsync(user)
                    | _ -> failwith "Parameter to change can not be recognized"
                match updateResult.Succeeded with
                | true ->
                    let signInManager = context.GetService<SignInManager<IdentityUser>>()
                    do! signInManager.SignOutAsync()
                    let! result = signInManager.SignInAsync(user,isPersistent = true)
                    return ChangeParamSuccess (updateResult.ToString())
                | false -> return ChangeParamFail (updateResult.Errors |> Seq.map (fun  x-> x.Code + " - " + x.Description) |> String.concat ". ")
            | false -> return ChangeParamFail "Error 401 Access Denied"
        } |> fun x -> x.Result
    else ChangeParamFail "Error 401 Access Denied"

let adminChangeUserParams (loginModel:LoginModel) (userInput:User) (userParameter:UserParameters) (input:string) (context: HttpContext) =
    let contextUserAuthentificationLevel =
        context.User.Claims |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> snd |> AuxFunctions.authentificationLevelByRole
    if context.User.HasClaim (ClaimTypes.Role,"Developer") || context.User.HasClaim (ClaimTypes.Role,"Admin") || context.User.HasClaim (ClaimTypes.Role,"UserManager")
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
                let! getRole = (userManager.GetClaimsAsync(user))
                let checkAuthentification =
                    if AuxFunctions.authentificationLevelByRole (string userInput.Role) >= contextUserAuthentificationLevel then failwith "Error 401 Access Denied; 01"
                let! updateResult =
                    match userParameter with
                    | Email -> userManager.SetEmailAsync(user,input)
                    | Username -> userManager.SetUserNameAsync(user,input)
                    | Password ->
                        let updateResult =
                            let hashPw = userManager.PasswordHasher.HashPassword(user, input)
                            user.PasswordHash <- hashPw
                        userManager.UpdateAsync(user)
                    | Role ->
                        let role = getRole |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> snd
                        let checkNewRole =
                            if AuxFunctions.authentificationLevelByRole input >= contextUserAuthentificationLevel then failwith "Error 401 Access Denied; 02"
                        userManager.ReplaceClaimAsync(user, new Claim(ClaimTypes.Role,role), new Claim(ClaimTypes.Role, input))
                    | _ -> failwith "Parameter to change can not be recognized"
                match updateResult.Succeeded with
                | true ->
                    if loginModel.Username = userInput.Username
                    then 
                        let signInManager = context.GetService<SignInManager<IdentityUser>>()
                        do! signInManager.SignOutAsync()
                        let! result = signInManager.SignInAsync(user,isPersistent = true)
                        return ChangeParamSuccess (updateResult.ToString())
                    else return ChangeParamSuccess (updateResult.ToString())
                | false -> return ChangeParamFail (updateResult.Errors |> Seq.map (fun  x-> x.Code + " - " + x.Description) |> String.concat ". ")
            | false -> return ChangeParamFail "Error 401 Access Denied"
        } |> fun x -> x.Result
    else ChangeParamFail "Error 401 Access Denied; 00"

let googleUserHandler (ctx: HttpContext) =
    let nameClaim = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Name)
    let emailClaim = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Email)
    if ctx.User.Identity.IsAuthenticated
    then
        {Username = nameClaim.Value; Email = emailClaim.Value; Role = Guest}
    else
        failwith "googleUserHandler returned an error. User not authenticated."

let gitHubUserHandler (ctx: HttpContext) =
    let nameClaim = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Name)
    let emailClaim =
        if ctx.User.HasClaim (fun c -> c.Type = ClaimTypes.Email)
        then ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Email) |> fun x -> x.Value
        else "Not puplicly available. If you want to portray your Email you need to change your GitHub account settings."
    if ctx.User.Identity.IsAuthenticated
    then
        {Username = nameClaim.Value; Email = emailClaim; Role = Guest}
    else
        failwith "googleUserHandler returned an error. User not authenticated."

let dotnetGetUser (context: HttpContext) =
    
    if isGoogleUser context
    then
        googleUserHandler context
    elif isGitHubUser context
    then gitHubUserHandler context
    else 
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
    let prevSignOut (ctx: HttpContext) =
        task {
            do! ctx.SignOutAsync("Cookies")
            return ctx
        } |> fun x -> x.Result
    task {
        let signInManager = (prevSignOut context).GetService<SignInManager<IdentityUser>>()
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
            let! addingClaims2 = userManager.AddClaimAsync(user, new Claim(ClaimTypes.AuthenticationMethod, "Registration"))
            match addingClaims.Succeeded,addingClaims2.Succeeded with
            | false,false -> return (RegisterFail (showErrors result.Errors))
            | true,true  ->
                let signInManager = context.GetService<SignInManager<IdentityUser>>()
                do! signInManager.SignInAsync(user, true)
                return RegisterSuccess "Registration Successful"
            | _,_ -> return (RegisterFail (showErrors result.Errors))
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
                let! addingClaims2 = userManager.AddClaimAsync(user, new Claim(ClaimTypes.AuthenticationMethod, "Registration"))
                match addingClaims.Succeeded,addingClaims2.Succeeded with
                | false,false -> return (RegisterFail (showErrors result.Errors))
                | true,true  ->
                    return RegisterSuccess "Registration Successful"
                | _,_ -> return (RegisterFail (showErrors result.Errors))
        } |> fun x -> x.Result