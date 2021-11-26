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
        NewProfileInfo: User option
    } with
        static member init(?userInfo) = {
            NewProfileInfo = userInfo
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

[<RequireQualifiedAccess>]
type PageModel =
| Todo of Todo.Model 
| Login of Login.Model
| Signup of Signup.Model
| Profile of Profile.Model
| AuthTest of AuthTest.Model

type UserState = {
    LoggedIn    : bool
    User        : User option
} with
    static member init = {
        LoggedIn    = false
        User        = None
    }

type Model = {
    NavbarMenuState : bool
    UserState       : UserState
    PageModel       : PageModel
}