namespace Shared

type Counter = { Value : int }

module ServiceHelpers =

    let ServiceMail = "Example@email.de"

module OAuthSigninPaths =

    let googleOAuth = "/api/google-auth"
    let githubOAuth = "/api/github-auth"
    let orcidOAuth = "/api/orcid-auth"

// allowed roles a user could have
type ActiveUserRoles =
| Developer
| Admin
| UserManager
| User
| Guest
| All
| NoRole

type ExternalLogin = {
    IsTrue : bool
    IsUsernameSet : bool
    }

type User = {
    Username : string
    Email : string
    Role : ActiveUserRoles
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
    adminRegisterUser : RegisterModel*ActiveUserRoles -> Async<DotnetRegisterResults>
    adminDeleteAccount : LoginModel * User -> Async<DotnetDeleteAccountResults>
    adminChangeUserParameters : LoginModel * User * UserParameters * string -> Async<DotnetChangeParameterResults>
}

module AuxFunctions =

    let stringToRoles (str:string) =
        match str with
        | "Developer" -> Developer
        | "Admin" -> Admin
        | "UserManager" -> UserManager
        | "User" -> User
        | _ -> Guest

    let authentificationLevelByUser (user:User option)=
        if user.IsNone then 0 else
        match user.Value.Role with
        | Developer -> 10
        | Admin -> 8
        | UserManager -> 5
        | User -> 2
        | _ -> 0

    let authentificationLevelByRole (role:string)=
        match role with
        | "Developer" -> 10
        | "Admin" -> 8
        | "UserManager" -> 5
        | "User" -> 2
        | _ -> 0
