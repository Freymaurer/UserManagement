module PreviousVersionIdentityFunctions

open Shared

open Giraffe

open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open FSharp.Control.Tasks
open System.Security.Claims
open System.Text


/// !! Attention !! ///
/// All functions in this file were straight up copy-pasted from the previous version originally used in SAFE v1.
/// Some functions were updated and streamlined to the SAFE v3 and can be found in AdminIdentity.fs and UserIdentity.fs
/// I kept this file as reference for anyone wanting to base their auth on these examples.
/// If you port any of the below functions to the newer version PLEASE open a PR/Issue at https://github.com/Freymaurer/UserManagement


//let roleAuthArray =
//    [|
//        Developer
//        Admin
//        User
//    |]

///// a function to check if userRole should be allowed to apply changes to userRole2. Returns 'true' if role level is equal or higher than role2. 
//let checkRoleAuth userRole userRole2=
//    let role1 = Array.findIndex (fun x -> x = userRole) roleAuthArray
//    let role2 = Array.findIndex (fun x -> x = userRole2) roleAuthArray
//    // lower index = higher role auth
//    role1 <= role2

//// define custom claims
//module CustomClaims =
//    let LoginMethod = "LoginMethod"
//    let IsUsernameSet = "IsUsernameSet"

//// define LoginMethods for custom claim "LoginMethod"
//module LoginMethods =
//    let LocalAuthority = "LOCAL AUTHORITY"
//    let Github = "GitHub"
//    let Google = "Google"

//let showErrors (errors : IdentityError seq) =
//    errors
//    |> Seq.fold (fun acc err ->
//        sprintf "Code: %s, Description: %s" err.Code err.Description
//        |> acc.AppendLine : StringBuilder) (StringBuilder(""))
//    |> (fun x -> x.ToString())

//open Microsoft.AspNetCore.Authentication
//open Microsoft.AspNetCore.WebUtilities

///// https://docs.microsoft.com/de-de/aspnet/core/security/authentication/identity?view=aspnetcore-3.1&tabs=visual-studio
//let dotnetLogin (user:LoginModel) (contextPre: HttpContext) =
//    let prevSignOut (ctx: HttpContext) =
//        task {
//            do! ctx.SignOutAsync("Cookies")
//            return ctx
//        } |> fun x -> x.Result
//    let context = prevSignOut contextPre
//    match user.Username,user.Password with
//    | "","" -> LoginFail "Password and Username are empty"
//    | "",_ -> LoginFail "Username is empty"
//    | _,"" -> LoginFail "Password is empty"
//    | _,_ ->
//        task {
//            let signInManager = context.GetService<SignInManager<IdentityUser>>()
//            let! result = signInManager.PasswordSignInAsync(user.Username, user.Password, true, true)
//            match result.Succeeded with
//            | true ->
//               return LoginSuccess
//            | false -> return LoginFail (result.ToString())
//        } |> fun x -> x.Result

//let dotnetDeleteAccount (loginModel:LoginModel) (context: HttpContext) =
//    if context.User.HasClaim (ClaimTypes.Name,loginModel.Username)
//    then
//        task {
//            let signInManager = context.GetService<SignInManager<IdentityUser>>()
//            let! result = signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, true, false)
//            match result.Succeeded with
//            | true ->
//                let userManager = context.GetService<UserManager<IdentityUser>>()
//                let! user = userManager.GetUserAsync context.User
//                let! deleteResult = userManager.DeleteAsync user
//                match deleteResult.Succeeded with
//                | true -> return DeleteSuccess
//                | false -> return DeleteFail (showErrors deleteResult.Errors)
//            | false -> return DeleteFail (result.ToString())
//        } |> fun x -> x.Result
//    else DeleteFail "Error 401 Access Denied"

//let adminDeleteAccount (loginModel:LoginModel) (userInput:User) (context: HttpContext) =
//    if context.User.HasClaim (ClaimTypes.Role,"Developer") || context.User.HasClaim (ClaimTypes.Role,"Admin") || context.User.HasClaim (ClaimTypes.Role,"Usermanager")
//    then
//        task {
//            let signInManager = context.GetService<SignInManager<IdentityUser>>()
//            let! result = signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, true, false)
//            match result.Succeeded with
//            | true ->
//                let userManager = context.GetService<UserManager<IdentityUser>>()
//                let! findByName = userManager.FindByNameAsync(userInput.Username)
//                let! findByEmail = userManager.FindByEmailAsync(userInput.Email)
//                let user = if findByName = findByEmail then findByName else failwith "Username and Email do not correlate"
//                let! deleteResult = userManager.DeleteAsync user
//                match deleteResult.Succeeded with
//                | true -> return DeleteSuccess
//                | false -> return DeleteFail (showErrors deleteResult.Errors)
//            | false -> return DeleteFail (result.ToString())
//        } |> fun x -> x.Result
//    else DeleteFail "Error 401 Access Denied"

//let dotnetChangeUserParams (loginModel:LoginModel) (userParameter:UserParameters) (input:string) (context: HttpContext) =
//    if context.User.HasClaim (ClaimTypes.Name,loginModel.Username)
//    then
//        task {
//            let signInManager = context.GetService<SignInManager<IdentityUser>>()
//            let! result = signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, true, false)
//            match result.Succeeded with
//            | true ->
//                let userManager = context.GetService<UserManager<IdentityUser>>()
//                let! user = userManager.GetUserAsync context.User
//                let! updateResult =
//                    match userParameter with
//                    | Email -> userManager.SetEmailAsync(user,input)
//                    | Username -> userManager.SetUserNameAsync(user,input)
//                    | Password ->
//                        let pwValidation = PasswordValidator<IdentityUser>().ValidateAsync(userManager, user, input)
//                        match pwValidation.Result.Succeeded with
//                        | true ->
//                            let hashPw = userManager.PasswordHasher.HashPassword(user, input)
//                            user.PasswordHash <- hashPw
//                            userManager.UpdateAsync(user)
//                        | false -> failwithf "%A" pwValidation.Result.Errors
//                    | _ -> failwith "Parameter to change can not be recognized"
//                match updateResult.Succeeded with
//                | true ->
//                    let signInManager = context.GetService<SignInManager<IdentityUser>>()
//                    do! signInManager.SignOutAsync()
//                    let! result = signInManager.SignInAsync(user,isPersistent = true)
//                    return ChangeParamSuccess
//                | false -> return ChangeParamFail (showErrors updateResult.Errors)
//            | false -> return ChangeParamFail (result.ToString())
//        } |> fun x -> x.Result
//    else ChangeParamFail "Error 401 Access Denied"

//let adminChangeUserParams (loginModel:LoginModel) (userInput:User) (userParameter:UserParameters) (input:string) (context: HttpContext) =
//    let contextUserRole =
//        context.User.Claims |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> snd |> Roles.stringToRole
//    if context.User.HasClaim (ClaimTypes.Role,"Developer") || context.User.HasClaim (ClaimTypes.Role,"Admin") || context.User.HasClaim (ClaimTypes.Role,"UserManager")
//    then
//        task {
//            let signInManager = context.GetService<SignInManager<IdentityUser>>()
//            let! result = signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, true, false)
//            match result.Succeeded with
//            | true ->
//                let userManager = context.GetService<UserManager<IdentityUser>>()
//                let! findByName = userManager.FindByNameAsync(userInput.Username)
//                let! findByEmail = userManager.FindByEmailAsync(userInput.Email)
//                let user = if findByName = findByEmail then findByName else failwith "Username and Email do not correlate"
//                let! getRole =
//                    userManager.GetClaimsAsync(user)
//                /// check if the user attempting to change the account should have auth over said the other user. 
//                let checkAuthentification =
//                    if checkRoleAuth contextUserRole userInput.Role |> not then failwith "Error 401 Access Denied; 01"
//                let! updateResult =
//                    match userParameter with
//                    | Email -> userManager.SetEmailAsync(user,input)
//                    | Username -> userManager.SetUserNameAsync(user,input)
//                    | Password ->
//                        let updateResult =
//                            let hashPw = userManager.PasswordHasher.HashPassword(user, input)
//                            user.PasswordHash <- hashPw
//                        userManager.UpdateAsync(user)
//                    | Role ->
//                        /// check if the user attempting to change the role of another user should be allowed to give access to the role he wants to set.
//                        let checkNewRole =
//                            let inputToRole =
//                                Roles.stringToRole input
//                            if checkRoleAuth contextUserRole inputToRole |> not then failwith "Error 401 Access Denied; 02"
//                        /// get current role, as it's needed to replace it with new role.
//                        let role = getRole |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> snd
//                        userManager.ReplaceClaimAsync(user, new Claim(ClaimTypes.Role,role), new Claim(ClaimTypes.Role, input))
//                match updateResult.Succeeded with
//                | true ->
//                    if loginModel.Username = userInput.Username
//                    then
//                        let signInManager = context.GetService<SignInManager<IdentityUser>>()
//                        do! signInManager.SignOutAsync()
//                        let! result = signInManager.SignInAsync(user,isPersistent = true)
//                        return ChangeParamSuccess
//                    else return ChangeParamSuccess
//                | false -> return ChangeParamFail (showErrors updateResult.Errors)
//            | false -> return ChangeParamFail (result.ToString())
//        } |> fun x -> x.Result
//    else ChangeParamFail "Error 401 Access Denied"

//let dotnetGetUser (context: HttpContext) =
//        task {
//            let userManager = context.GetService<UserManager<IdentityUser>>()
//            let! user = userManager.GetUserAsync context.User
//            let claims = (userManager.GetClaimsAsync(user)) |> fun x -> x.Result |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq
//            let extLogin =
//                claims |> List.tryFind (fun (claimType,value) -> claimType = CustomClaims.IsUsernameSet)
//                |> fun x -> if x.IsSome then { IsTrue = true; IsUsernameSet = bool.Parse(x.Value |> snd) } else { IsTrue = false; IsUsernameSet = true }
//            let role = claims |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> (snd >> Roles.stringToRole)
//            let origin = claims |> List.find (fun (claimType,value) -> claimType = CustomClaims.LoginMethod) |> snd
//            return { Username = user.UserName; Email = user.Email; Role = role; AccountOrigin = origin; UniqueId = user.Id; ExtLogin = extLogin }
//        } |> fun x -> x.Result

//let dotnetGetAllUsers (context: HttpContext) =
//    task {
//        let userManager = context.GetService<UserManager<IdentityUser>>()
//        let userList = userManager.Users |> Array.ofSeq // Possible error
//        let createUser =
//            userList
//            |> Array.map (
//                fun x ->
//                    let claims = userManager.GetClaimsAsync(x) |> fun x -> x.Result |> Seq.map (fun x -> x.Type,x.Value) |> List.ofSeq
//                    let role = claims |> List.find (fun (claimType,value) -> claimType = ClaimTypes.Role) |> (snd >> Roles.stringToRole)
//                    let origin = claims |> List.find (fun (claimType,value) -> claimType = CustomClaims.LoginMethod) |> snd
//                    let extLogin =
//                        claims |> List.tryFind (fun (claimType,value) -> claimType = ClaimTypes.NameIdentifier)
//                        |> fun x -> if x.IsSome then { IsTrue = true; IsUsernameSet = bool.Parse(x.Value |> snd) } else { IsTrue = false; IsUsernameSet = true }
//                    {
//                        Username = x.UserName;
//                        Email = x.Email;
//                        Role = role
//                        AccountOrigin = origin
//                        UniqueId = x.Id
//                        ExtLogin = extLogin
//                    }
//                )
//        return createUser
//    } |> fun x -> x.Result

//let dotnetUserLogOut (context: HttpContext) =
//    task {
//        let signInManager = context.GetService<SignInManager<IdentityUser>>()
//        do! signInManager.SignOutAsync()
//        return LogoutSuccess
//    } |> fun x -> x.Result

//let dotnetRegistration (registerModel:RegisterModel) (context: HttpContext) =
//    printfn "enter register"
//    task {
//        let user = IdentityUser(UserName = registerModel.Username, Email = registerModel.Email)
//        let userManager = context.GetService<UserManager<IdentityUser>>()
//        let! result = userManager.CreateAsync(user, registerModel.Password)
//        match result.Succeeded with
//        | false -> return (RegisterFail (showErrors result.Errors))
//        | true  ->
//            //let! emailConfirmToken =
//            //    userManager.GenerateEmailConfirmationTokenAsync(user)
//            //let encodedEmailConfirmtoken =
//            //    WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmToken))
//            /// check if already users exist. If not create admin. If there is only 1 user, then it is the one just created 4 lines above
//            let userCount = userManager.Users |> Array.ofSeq |> Array.length // Possible error
//            let role = if userCount = 1 then Roles.Admin else Roles.Guest
//            let! addingClaims = userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, (string role)))
//            ///
//            let! addingClaims2 =
//                userManager.AddClaimAsync(user, new Claim(CustomClaims.LoginMethod, LoginMethods.LocalAuthority))
//            match addingClaims.Succeeded,addingClaims2.Succeeded with
//            | false,false -> return (RegisterFail (showErrors result.Errors))
//            | true,true  ->
//                let signInManager = context.GetService<SignInManager<IdentityUser>>()
//                do! signInManager.SignInAsync(user, true)
//                return RegisterSuccess
//            | _,_ -> return (RegisterFail (showErrors result.Errors))
//    } |> fun x -> x.Result

//let adminUserRegistration (registerModel:RegisterModel) (role:Roles) (context: HttpContext) =
//    if role = Developer || role = Guest
//    then RegisterFail "Cannot create a new user with the given role."
//    else
//        task {
//            let  user        = IdentityUser(UserName = registerModel.Username, Email = registerModel.Email)
//            let  userManager = context.GetService<UserManager<IdentityUser>>()
//            let! result      = userManager.CreateAsync(user, registerModel.Password)
//            match result.Succeeded with
//            | false -> return (RegisterFail (showErrors result.Errors))
//            | true  ->
//                let! addingClaims = userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, string role))
//                let! addingClaims2 = userManager.AddClaimAsync(user, new Claim(CustomClaims.LoginMethod, LoginMethods.LocalAuthority))
//                match addingClaims.Succeeded,addingClaims2.Succeeded with
//                | false,false -> return (RegisterFail (showErrors result.Errors))
//                | true,true  ->
//                    return RegisterSuccess
//                | _,_ -> return (RegisterFail (showErrors result.Errors))
//        } |> fun x -> x.Result

//// returns origin of user account (think: oauth or local authority)
//let checkAccountOrigin (ctx: HttpContext) =
//    let checkOAuth = ctx.User.HasClaim (fun c -> c.Type = CustomClaims.LoginMethod && c.Value = LoginMethods.LocalAuthority)
//    if checkOAuth = true
//    then LoginMethods.LocalAuthority
//    else
//        ctx.User.Claims
//        |> Seq.map (fun x -> x.Issuer)
//        |> Seq.filter (fun x -> x <> LoginMethods.LocalAuthority)
//        |> Seq.groupBy (fun x -> x)
//        |> fun x -> if Seq.length x > 1 then failwith "Unknown Claims issuer!" else x |> (Seq.head >> fst)

//module OAuth =

//    //https://github.com/giraffe-fsharp/Giraffe/blob/master/src/Giraffe/Auth.fs#L23
//    let private challenge (scheme : string) (redirectUri : string) : HttpHandler =
//        fun (next : HttpFunc) (ctx : HttpContext) ->
//            task {
//                let signinManager = ctx.GetService<SignInManager<IdentityUser>>()
//                let properties = signinManager.ConfigureExternalAuthenticationProperties(scheme, redirectUri)
//                do! ctx.ChallengeAsync(
//                        scheme,
//                        properties
//                    )
//                return! next ctx
//            }

//    let gitHubAuth : HttpHandler =
//        challenge "GitHub" "http://localhost:8080"

//    let googleAuth : HttpHandler =
//        challenge "Google" "http://localhost:8080"

//    let orcidAuth : HttpHandler =
//        challenge "Orcid" "http://localhost:8080"

//    type Message = {
//        Text    :   string
//    }

//    let externalLoginCallback : HttpHandler =
//        fun (next : HttpFunc) (ctx : HttpContext) ->
//            task {
//                let signinManager = ctx.GetService<SignInManager<IdentityUser>>()
//                let uniqueIdent = ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.NameIdentifier) |> fun x -> x.Value
//                let issuer = checkAccountOrigin ctx
//                let! signInResult = signinManager.ExternalLoginSignInAsync(issuer,uniqueIdent,true)
//                match signInResult.Succeeded with
//                |false ->
//                    let emailClaim =
//                        if ctx.User.HasClaim (fun c -> c.Type = ClaimTypes.Email)
//                        then ctx.User.FindFirst (fun c -> c.Type = ClaimTypes.Email) |> fun x -> x.Value
//                        else issuer + "@" + uniqueIdent
//                    let user = new IdentityUser(userName = emailClaim, Email = emailClaim)
//                    let userManager = ctx.GetService<UserManager<IdentityUser>>()
//                    let! createResult = userManager.CreateAsync(user)
//                    match createResult.Succeeded with
//                        | true ->
//                            let info = UserLoginInfo(issuer,uniqueIdent,issuer)
//                            let! addLoginResult = userManager.AddLoginAsync(user,info)
//                            match addLoginResult.Succeeded with
//                            | true ->
//                                let! addClaims =
//                                    userManager.AddClaimsAsync(user,[
//                                            Claim(
//                                                ClaimTypes.Role,"Guest",ClaimValueTypes.String,LoginMethods.LocalAuthority
//                                            );
//                                            Claim(
//                                                CustomClaims.LoginMethod,issuer,ClaimValueTypes.String,LoginMethods.LocalAuthority
//                                            )
//                                            Claim(
//                                                CustomClaims.IsUsernameSet,"false",ClaimValueTypes.Boolean,LoginMethods.LocalAuthority
//                                            )
//                                    ])
//                                let! signinResult = signinManager.SignInAsync(user,true)
//                                return! next ctx
//                            | false -> return failwith "ExternalLoginFail AddLogin failed"
//                        | false -> return failwith "ExternalLoginFail CreateUser failed"
//                | true ->
//                    return! next ctx
//            }

//    let addUsernameToExtLoginFunc (username:string) (ctx:HttpContext) =
//        task {
//            let userManager = ctx.GetService<UserManager<IdentityUser>>()
//            let! user = userManager.GetUserAsync(ctx.User)
//            let! addUserNameResult = userManager.SetUserNameAsync(user,username)
//            match addUserNameResult.Succeeded with
//            | true ->
//                let! changeClaimResult = userManager.ReplaceClaimAsync(user, Claim(CustomClaims.IsUsernameSet,"false"), Claim(CustomClaims.IsUsernameSet,"true"))
//                match changeClaimResult.Succeeded with
//                | true ->
//                    let signinManager = ctx.GetService<SignInManager<IdentityUser>>()
//                    let! signout = signinManager.SignOutAsync()
//                    let! signin = signinManager.SignInAsync(user,true)
//                    return ChangeParamSuccess
//                | false ->
//                    return changeClaimResult.Errors.ToString() |> ChangeParamFail
//            | false ->
//                return addUserNameResult.Errors.ToString() |> ChangeParamFail
//        } |> fun x -> x.Result