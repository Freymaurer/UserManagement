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

let curry f a b = f (a,b)

module Identity =

    open IdentityTypes

    // Might be possible to refactor most "Result<_,_> logics to ignore the error part.
    // In this case change Server functions to remove Ok (res) and replace Error (errors) with failwith $"{errors}"
    // didnt't test it tough

    type Msg =
    | LoginRequest of LoginInfo
    | LoginResponse of Result<unit,string>
    | SignupRequest of SignupInfo
    | SignupResponse of Result<unit,string>
    | LogoutRequest
    | LogoutResponse of unit
    | GetActiveUserRequest
    | GetActiveUserResponse of User
    | UpdateUserProfileRequest of IdentityTypes.User
    | UpdateUserProfileResponse of Result<IdentityTypes.User,string>
    // this is only for testing during dev.
    | GetNumRequest

module Todo =
    type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

module AuthTest =
    type Msg =
    | GetUserHelloRequest
    | GetUserHelloResponse of string
    | GetAdminHelloRequest
    | GetAdminHelloResponse of string

module Login =
    type Msg =
    | UpdateLoginInfo of IdentityTypes.LoginInfo

module Signup =
    type Msg =
    | UpdateSignupInfo          of IdentityTypes.SignupInfo
    | UpdatePasswordDuplicate   of string

module Profile =
    type Msg =
    | UpdateNewProfileInfo of IdentityTypes.User

type Msg =
    | UpdateNavbarMenuState of bool
    | UpdatePage of Routing.Route
    | GenericError of Cmd<Msg> * exn
    | GenericLog of string
    | IdentityMsg of Identity.Msg
    | TodoMsg of Todo.Msg
    | LoginMsg of Login.Msg
    | SignupMsg of Signup.Msg
    | ProfileMsg of Profile.Msg
    | AuthTestMsg of AuthTest.Msg