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

let login (user:LoginInfo) (contextPre: HttpContext) =
    printfn "hit"
    let signOutPrevious (ctx: HttpContext) =
        task {
            do! ctx.SignOutAsync("Cookies")
            return ctx
        } |> fun x -> x.Result
    let context = signOutPrevious contextPre
    match user.Username,user.Password with
    | "","" -> Error "Password and Username are empty"
    | "",_ -> Error "Username is empty"
    | _,"" -> Error "Password is empty"
    | _,_ ->
        task {
            let signInManager = context.GetService<SignInManager<IdentityUser>>()
            let! result = signInManager.PasswordSignInAsync(user.Username, user.Password, true, false)
            match result.Succeeded with
            | true  -> return Ok()
            | false -> return Error "Invalid login data."
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