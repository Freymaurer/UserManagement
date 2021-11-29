module AdminIdentity

open Giraffe
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open FSharp.Control.Tasks
open System.Security.Claims
open System.Text


open IdentityTypes
open Shared

open UserIdentity

let getUsers (context: HttpContext) =
    task {
        let userManager = context.GetService<UserManager<IdentityUser>>()
        let! user = userManager.GetUserAsync context.User
        let claims = (userManager.GetClaimsAsync(user)) |> fun x -> x.Result |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq
        let role = claims |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> (snd >> Roles.ofString)
        match role with
        | Roles.Admin | Roles.Developer ->
            let userList = userManager.Users |> Array.ofSeq // Possible error
            let createUser =
                userList
                |> Array.map (
                    fun x ->
                        let claims = userManager.GetClaimsAsync(x) |> fun x -> x.Result |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq
                        let role = claims |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> (snd >> Roles.ofString)
                        let origin = claims |> List.find (fun (claimType,value) -> claimType = CustomClaims.LoginMethod) |> snd
                        let extLogin =
                            claims |> List.tryFind (fun (claimType,value) -> claimType = ClaimTypes.NameIdentifier)
                            |> fun x -> if x.IsSome then { IsTrue = true; IsUsernameSet = bool.Parse(x.Value |> snd) } else { IsTrue = false; IsUsernameSet = true }
                        {
                            Username = x.UserName;
                            Email = x.Email;
                            Role = role
                            AccountOrigin = origin
                            UniqueId = x.Id
                            ExtLogin = extLogin
                        }
                    )
            return createUser
        | anythingElse ->
            return failwith "Error 401, not authorized to access this information"
    } |> fun x -> x.Result