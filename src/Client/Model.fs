module rec Model

open Shared
open IdentityTypes
open Elmish

module Login =

    type Model = {
        LoginInfo: LoginInfo
    } with
        static member init = {
            LoginInfo = LoginInfo.empty
        }

module Signup =

    type Model = {
        SignupInfo          : SignupInfo
        PasswordDuplicate   : string
    } with
        static member init = {
            SignupInfo          = SignupInfo.empty
            PasswordDuplicate   = ""
        }

module Todo =
    type Model = {
        Todos: Todo list;
        Input: string
    } with
        static member init = {
            Todos = []
            Input = ""
        }

module Profile =
    type Model = {
        NewProfileInfo      : User option
        NewPassword         : string
        NewPasswordCheck    : string
    } with
        static member init(?userInfo) = {
            NewProfileInfo      = userInfo
            NewPassword         = ""
            NewPasswordCheck    = ""
        }

module AuthTest =
    [<RequireQualifiedAccessAttribute>]
    type ResponseMsg =
    | User of string
    | Admin of string

    type Model = {
        Response : ResponseMsg option
    } with
    static member init() = {
        Response = None
    }

module AdminUsers =
    type Model = {
        Users: IdentityTypes.User []
    } with
        static member init() = {
            Users = [||]
        }

[<RequireQualifiedAccess>]
type PageModel =
| Todo of Todo.Model 
| Login of Login.Model
| Signup of Signup.Model
| Profile of Profile.Model
| AuthTest of AuthTest.Model
| AdminUsers of AdminUsers.Model

type UserState = {
    LoggedIn            : bool
    User                : User option
    /// this field is used to toggle confirm password modal for important auth functions
    /// value is follow msg to be piped into.
    ShowPasswordModal   : (IdentityTypes.LoginInfo -> Messages.Msg) option
    PasswordModalPw     : string
} with
    static member init = {
        LoggedIn            = false
        User                = None
        ShowPasswordModal   = None
        PasswordModalPw     = ""
    }

type Model = {
    NavbarMenuState : bool
    UserState       : UserState
    PageModel       : PageModel
}