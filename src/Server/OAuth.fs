module OAuth

open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open FSharp.Control.Tasks
open System.Security.Claims
open System.Text
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.WebUtilities

open Giraffe

//https://github.com/giraffe-fsharp/Giraffe/blob/master/src/Giraffe/Auth.fs#L23
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

let gitHubAuth : HttpHandler =
    challenge "GitHub" StaticStrings.SameSiteUrls.PageUrl

let googleAuth : HttpHandler =
    challenge "Google" StaticStrings.SameSiteUrls.PageUrl

let orcidAuth : HttpHandler =
    challenge "Orcid" StaticStrings.SameSiteUrls.PageUrl

open UserIdentity

// returns origin of user account (think: oauth or local authority)
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

let externalLoginCallback : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let signinManager = ctx.GetService<SignInManager<IdentityUser>>()
            let uniqueIdent = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.NameIdentifier) |> fun x -> x.Value
            let issuer = checkAccountOrigin ctx
            let! signInResult = signinManager.ExternalLoginSignInAsync(issuer,uniqueIdent,true)
            match signInResult.Succeeded with
            |false ->
                let emailClaim =
                    if ctx.User.HasClaim (fun c -> c.Type = ClaimTypes.Email)
                    then ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Email) |> fun x -> x.Value
                    else issuer + "@" + uniqueIdent
                let user = new IdentityUser(userName = emailClaim, Email = emailClaim)
                let userManager = ctx.GetService<UserManager<IdentityUser>>()
                let! createResult = userManager.CreateAsync(user)
                match createResult.Succeeded with
                    | true ->
                        let info = UserLoginInfo(issuer,uniqueIdent,issuer)
                        let! addLoginResult = userManager.AddLoginAsync(user,info)
                        match addLoginResult.Succeeded with
                        | true ->
                            let! addClaims =
                                userManager.AddClaimsAsync(user,[
                                        Claim(
                                            ClaimTypes.Role,string IdentityTypes.User,ClaimValueTypes.String,LoginMethods.LocalAuthority
                                        );
                                        Claim(
                                            CustomClaims.LoginMethod,issuer,ClaimValueTypes.String,LoginMethods.LocalAuthority
                                        )
                                        Claim(
                                            CustomClaims.IsUsernameSet,"false",ClaimValueTypes.Boolean,LoginMethods.LocalAuthority
                                        )
                                ])
                            let! signinResult = signinManager.SignInAsync(user,true)
                            return! next ctx
                        | false -> return failwith "ExternalLoginFail AddLogin failed"
                    | false -> return failwith "ExternalLoginFail CreateUser failed"
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
                return Ok ()
            | false ->
                return Error <| changeClaimResult.Errors.ToString()
        | false ->
            return Error <| addUserNameResult.Errors.ToString()
    } |> fun x -> x.Result