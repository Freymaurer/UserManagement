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
            Todos = [];
            Input = ""
        }

[<RequireQualifiedAccess>]
type PageModel =
| Home of Todo.Model
| Login of Login.Model
| Signup of Signup.Model

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