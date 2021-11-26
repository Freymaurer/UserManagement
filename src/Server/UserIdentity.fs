module UserIdentity

open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Authentication
open FSharp.Control.Tasks
open System.Security.Claims
open System.Text
open Giraffe

open IdentityTypes

// define custom claims
module CustomClaims =
    let LoginMethod = "LoginMethod"
    let IsUsernameSet = "IsUsernameSet"

module LoginMethods =
    let LocalAuthority = "LOCAL AUTHORITY"
    let Github = "GitHub"
    let Google = "Google"

/// Use this function for conditional if..else.. cases which return IdentityResults.
/// Not sure if this is good style.
let identitySuccess = task {return IdentityResult.Success}

let showErrors (errors : IdentityError seq) =
    errors
    |> Seq.fold (fun acc err ->
        sprintf "Code: %s, Description: %s" err.Code err.Description
        |> acc.AppendLine : StringBuilder) (StringBuilder(""))
    |> (fun x -> x.ToString())

let signup (registerModel:SignupInfo) (context: HttpContext) =
    task {
        let user = IdentityUser(UserName = registerModel.Username, Email = registerModel.Email)
        let userManager = context.GetService<UserManager<IdentityUser>>()
        let! result = userManager.CreateAsync(user, registerModel.Password)
        match result.Succeeded with
        | false -> return failwith (showErrors result.Errors)
        | true  ->
            //let! emailConfirmToken =
            //    userManager.GenerateEmailConfirmationTokenAsync(user)
            //let encodedEmailConfirmtoken =
            //    WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmToken))
            /// check if already users exist. If not create admin. If there is only 1 user, then it is the one just created 4 lines above
            let userCount = userManager.Users |> Array.ofSeq |> Array.length // Possible error
            let role = if userCount = 1 then Roles.Admin else Roles.User
            let! addingClaims = userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, (string role)))
            ///
            let! addingClaims2 =
                userManager.AddClaimAsync(user, new Claim(CustomClaims.LoginMethod, LoginMethods.LocalAuthority))
            match addingClaims.Succeeded,addingClaims2.Succeeded with
            | false,false -> return failwith (showErrors result.Errors)
            | true,true  ->
                let signInManager = context.GetService<SignInManager<IdentityUser>>()
                do! signInManager.SignInAsync(user, true)
                return ()
            | _,_ -> return failwith (showErrors result.Errors)
    } |> fun x -> x.Result

let login (user:LoginInfo) (contextPre: HttpContext) =
    let signOutPrevious (ctx: HttpContext) =
        task {
            do! ctx.SignOutAsync("Cookies")
            return ctx
        } |> fun x -> x.Result
    let context = signOutPrevious contextPre
    match user.Username,user.Password with
    | "","" -> failwith "Password and Username are empty"
    | "",_ -> failwith "Username is empty"
    | _,"" -> failwith "Password is empty"
    | _,_ ->
        task {
            let signInManager = context.GetService<SignInManager<IdentityUser>>()
            let! result = signInManager.PasswordSignInAsync(user.Username, user.Password, true, false)
            match result.Succeeded with
            | true  -> return ()
            | false -> return failwith "Invalid login data."
        } |> fun x -> x.Result

let logout (context: HttpContext) =
    task {
        let signInManager = context.GetService<SignInManager<IdentityUser>>()
        do! signInManager.SignOutAsync()
        return ()
    } |> fun x -> x.Result

let getActiveUser (context: HttpContext) : User =
    task {
        let userManager = context.GetService<UserManager<IdentityUser>>()
        let! user = userManager.GetUserAsync context.User
        let claims = (userManager.GetClaimsAsync(user)) |> fun x -> x.Result |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq
        let extLogin =
            claims |> List.tryFind (fun (claimType,value) -> claimType = CustomClaims.IsUsernameSet)
            |> fun x -> if x.IsSome then { IsTrue = true; IsUsernameSet = bool.Parse(x.Value |> snd) } else { IsTrue = false; IsUsernameSet = true }
        let role = claims |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> (snd >> Roles.ofString)
        let origin = claims |> List.find (fun (claimType,value) -> claimType = CustomClaims.LoginMethod) |> snd
        return { Username = user.UserName; Email = user.Email; Role = role; AccountOrigin = origin; UniqueId = user.Id; ExtLogin = extLogin }
    } |> fun x -> x.Result

let updateUserProfile (newUserInfo:User) (context: HttpContext) =
    task {
        let userManager = context.GetService<UserManager<IdentityUser>>()
        let! user = userManager.GetUserAsync context.User
        let! updateEmail =
            if newUserInfo.Email <> user.Email then userManager.SetEmailAsync(user,newUserInfo.Email) else identitySuccess
        let! updateUserName =
            if newUserInfo.Username <> user.UserName then userManager.SetUserNameAsync(user,newUserInfo.Username) else identitySuccess
        match updateEmail.Succeeded, updateUserName.Succeeded with
        | true, true ->
            let signInManager = context.GetService<SignInManager<IdentityUser>>()
            do! signInManager.SignOutAsync()
            let! result = signInManager.SignInAsync(user,isPersistent = true)
            return newUserInfo
        | true, false ->
            return failwith (showErrors updateUserName.Errors)
        | false, true ->
            return failwith (showErrors updateEmail.Errors)
        | false, false ->
            return failwith (Seq.concat [updateEmail.Errors; updateUserName.Errors] |> showErrors)
    } |> fun x -> x.Result

let updatePassword (loginInfo:LoginInfo) (newPassword:string) (context: HttpContext) =
    task {
        let userManager = context.GetService<UserManager<IdentityUser>>()
        let! user = userManager.GetUserAsync context.User
        /// Might be redundant, as i expect .ChangePasswordAsync(...) to check current password.
        let! isValidPw = userManager.CheckPasswordAsync(user,loginInfo.Password)
        match isValidPw with
        | true ->
            let! updatePassword =
                userManager.ChangePasswordAsync(user,loginInfo.Password,newPassword)
            match updatePassword.Succeeded with
            | true ->
                let signInManager = context.GetService<SignInManager<IdentityUser>>()
                do! signInManager.SignOutAsync()
                let! result = signInManager.SignInAsync(user,isPersistent = true)
                return ()
            | false ->
                return failwith (showErrors updatePassword.Errors)
        | false ->
            failwith "Invalid password."
    } |> fun x -> x.Result