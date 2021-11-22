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
    | SignupRequest of SignupInfo
    | SignupResponse of Result<unit,string>
    | LogoutRequest
    | LogoutResponse of unit
    | GetActiveUserRequest
    | GetActiveUserResponse of User
    // this is only for testing during dev.
    | GetNumRequest

module Todo =
    type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

module Login =
    type Msg =
    | UpdateLoginInfo of IdentityTypes.LoginInfo

module Signup =
    type Msg =
    | UpdateSignupInfo          of IdentityTypes.SignupInfo
    | UpdatePasswordDuplicate   of string

type Msg =
    | UpdateNavbarMenuState of bool
    | UpdatePage of Routing.Route
    | GenericError of Cmd<Msg> * exn
    | GenericLog of string
    | IdentityMsg of Identity.Msg
    | TodoMsg of Todo.Msg
    | LoginMsg of Login.Msg
    | SignupMsg of Signup.Msg