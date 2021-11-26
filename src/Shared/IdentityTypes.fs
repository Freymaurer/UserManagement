module IdentityTypes

type LoginInfo = {
    Username : string
    Password : string
} with
    static member create name pw = {
        Username = name 
        Password = pw
    }

    static member empty = {
        Username = "" 
        Password = ""
    }

type SignupInfo = {
    Username : string
    Password : string
    Email : string
} with
    static member empty = {
        Username = "" 
        Password = ""
        Email = ""
    }

type ExternalLogin = {
    IsTrue : bool
    IsUsernameSet : bool
}

type Roles =
| Developer
| Admin
| User
    static member ofString str =
        match str with
        | "Developer"   -> Developer
        | "Admin"       -> Admin
        | "User"        -> User
        | anythingElse  -> failwith $"Could not parse {anythingElse} to user role."

    member this.isAdmin = this = Developer || this = Admin 
        

type User = {
    Username : string
    Email : string
    Role : Roles
    AccountOrigin : string
    UniqueId : string
    ExtLogin : ExternalLogin
}