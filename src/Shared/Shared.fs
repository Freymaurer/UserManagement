namespace Shared

type Counter = { Value : int }

type ActiveUserRoles =
| Developer
| Admin
| UserManager
| User
| Guest
| All

type User = {
    Username : string
    Email : string
    Role : ActiveUserRoles
}

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

type DotnetLoginResults =
| LoginSuccess of string
| LoginFail of string

type DotnetLogOutResults =
| LogoutSuccess of string
| LogoutFail of string

type DotnetRegisterResults =
| RegisterSuccess of string
| RegisterFail of string

type DotnetDeleteAccountResults =
| DeleteSuccess of string
| DeleteFail of string

type DotnetChangeParameterResults =
| ChangeParamSuccess of string
| ChangeParamFail of string

module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type ICounterApi = {
    // returns initial count of 42
    initialCounter : unit -> Async<Counter>
    }

type IDotnetApi = {
    dotnetLogin : LoginModel -> Async<DotnetLoginResults>
    dotnetRegister : RegisterModel -> Async<DotnetRegisterResults>
    githubSignIn    : unit -> Async<string>
}

type IDotnetSecureApi = {
    getUserCounter : unit -> Async<Counter>
    dotnetGetUser : unit -> Async<User>
    dotnetUserLogOut : unit -> Async<DotnetLogOutResults>
    dotnetDeleteUserAccount : LoginModel -> Async<DotnetDeleteAccountResults>
    dotnetChangeUserParameters : LoginModel * UserParameters * string -> Async<DotnetChangeParameterResults>
}

type IAdminSecureApi = {
    dotnetGetAllUsers : unit -> Async<User []>
    adminRegisterUser : RegisterModel*ActiveUserRoles -> Async<DotnetRegisterResults>
    adminDeleteAccount : LoginModel * User -> Async<DotnetDeleteAccountResults>
    adminChangeUserParameters : LoginModel * User * UserParameters * string -> Async<DotnetChangeParameterResults>
}

type IOauthApi = {
    getUserFromGoogle : unit -> Async<string>
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