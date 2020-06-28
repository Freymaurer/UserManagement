namespace Shared

type Counter = { Value : int }

module ServiceHelpers =

    let ServiceMail = "Example@email.de"

module OAuthSigninPaths =

    let googleOAuth = "/api/google-auth"
    let githubOAuth = "/api/github-auth"
    let orcidOAuth = "/api/orcid-auth"

// allowed roles a user could have
type Roles =
| Developer
| Admin
| UserManager
| User
| Guest

    static member stringToRole (str:string) =
        match str with
        | "developer" | "Developer" -> Developer
        | "admin" | "Admin" -> Admin
        | "usermanager" | "UserManager" -> UserManager
        | "user" | "User" -> User
        | "guest" | "Guest" -> Guest
        | _ -> failwith "not a valid role"

    static member roleAuthArray =
        [|
            Developer
            Admin
            Roles.UserManager
            User
            Guest
        |]

    /// a function to check if userRole should be allowed to apply changes to userRole2. Returns 'true' if role level is equal or higher than role2. 
    static member checkRoleAuth userRole userRole2=
        let role1 = Array.findIndex (fun x -> x = userRole) Roles.roleAuthArray
        let role2 = Array.findIndex (fun x -> x = userRole2) Roles.roleAuthArray
        // lower index = higher role auth
        role1 <= role2


type ExternalLogin = {
    IsTrue : bool
    IsUsernameSet : bool
    }

type User = {
    Username : string
    Email : string
    Role : Roles
    AccountOrigin : string
    UniqueId : string
    ExtLogin : ExternalLogin
}

// used to determine which account value to change, e.g. when calling "dotnetChangeUserParameters"
type UserParameters =
|Username
|Password
|Email
|Role

type LoginModel = {
    Username : string
    Password : string
}

type RegisterModel = {
    Username : string
    Password : string
    Email : string
}

// server responses

type DotnetLoginResults =
| LoginSuccess
| LoginFail of string

type DotnetLogOutResults =
| LogoutSuccess
| LogoutFail of string

type DotnetRegisterResults =
| RegisterSuccess
| RegisterFail of string

type DotnetDeleteAccountResults =
| DeleteSuccess
| DeleteFail of string

type DotnetChangeParameterResults =
| ChangeParamSuccess
| ChangeParamFail of string

type DotnetExternalLoginResults =
| ExternalLoginSuccess
| ExternalLoginFail of string

module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type IUserApi = {
    dotnetLogin : LoginModel -> Async<DotnetLoginResults>
    dotnetRegister : RegisterModel -> Async<DotnetRegisterResults>
    getContextClaims : unit -> Async<string>
    initialCounter : unit -> Async<Counter>
}

type IDotnetSecureApi = {
    getUserCounter : unit -> Async<Counter>
    dotnetGetUser : unit -> Async<User>
    dotnetUserLogOut : unit -> Async<DotnetLogOutResults>
    dotnetDeleteUserAccount : LoginModel -> Async<DotnetDeleteAccountResults>
    dotnetChangeUserParameters : LoginModel * UserParameters * string -> Async<DotnetChangeParameterResults>
    addUsernameToExtLogin : string -> Async<DotnetChangeParameterResults>
}

type IAdminSecureApi = {
    dotnetGetAllUsers : unit -> Async<User []>
    adminRegisterUser : RegisterModel*Roles -> Async<DotnetRegisterResults>
    adminDeleteAccount : LoginModel * User -> Async<DotnetDeleteAccountResults>
    adminChangeUserParameters : LoginModel * User * UserParameters * string -> Async<DotnetChangeParameterResults>
}
