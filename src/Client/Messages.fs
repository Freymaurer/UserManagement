module rec Messages

open Shared
open Fable.Remoting.Client
open Fable.SimpleJson
open Elmish
open Model

type System.Exception with
    member this.GetPropagatedError() =
        match this with
        | :? ProxyRequestException as exn ->
            let response = exn.ResponseText |> Json.parseAs<{| error:string; ignored : bool; handled : bool |}>
            response.error
        | ex ->
            ex.Message

module Identity =

    open IdentityTypes

    type Msg =
    | LoginRequest of LoginInfo
    | LoginResponse of Result<unit,string>
    | GetActiveUserRequest
    | GetActiveUserResponse of User
    | GetNumRequest
    | LogoutRequest
    | LogoutResponse of unit

module Todo =
    type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

module Login =
    type Msg =
    | UpdateLoginInfo of IdentityTypes.LoginInfo

type Msg =
    | UpdateNavbarMenuState of bool
    | UpdatePageModel of PageModel
    | GenericError of Cmd<Msg> * exn
    | GenericLog of string
    | IdentityMsg of Identity.Msg
    | TodoMsg of Todo.Msg
    | LoginMsg of Login.Msg