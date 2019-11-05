module Client.State

open System

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Fulma
open Thoth.Json
open Fable.Core
open Shared
open Fable.FontAwesome

open Client.Types

// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = {
        Counter = None
        User = None
        Loading = true
        Authenticated = false
        ErrorMsg = None
        LoginModel = {Username = ""; Password = ""}
        RegisterModel = {Username = "";Password = "";Email = ""}
        ExtraReactElement = EmptyElement
        MainReactElement = Counter
        ShowMenuBool = false
    }
    let loadCountCmd =
        Cmd.OfAsync.perform
                Server.userApi.initialCounter
                ()
                InitialCountLoaded
    let logInCmd =
        Cmd.ofMsg DotnetGetUserRequest
    let aggregrateCmds =
        Cmd.batch [loadCountCmd; logInCmd]
    initialModel, aggregrateCmds

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel.Counter, msg with
        /// functions to manage counter
    | Some counter, Increment ->
        let nextModel = { currentModel with Counter = Some { Value = counter.Value + 1 } }
        nextModel, Cmd.none
    | Some counter, Decrement ->
        let nextModel = { currentModel with Counter = Some { Value = counter.Value - 1 } }
        nextModel, Cmd.none
    | _, InitialCountLoaded initialCount ->
        let nextModel = { currentModel with Counter = Some initialCount; Loading = false }
        nextModel, Cmd.none
        /// Menu Management Functions
    | _, ToggleMenu ->
        let nextModel = {
            currentModel with
                ShowMenuBool = if currentModel.ShowMenuBool = true then false else true
        }
        nextModel,Cmd.none
    | _, ChangeMainReactElement (newElement) ->
        let nextModel = {
            currentModel with
                MainReactElement = newElement
        }
        nextModel, Cmd.none
        /// functions to manage input fields for user log in
    | _ , UpdateLoginUsername (name:string) ->
        let nextModel = {
            currentModel with
                LoginModel = {currentModel.LoginModel with Username = name}
        }
        nextModel, Cmd.none
    | _ , UpdateLoginUserPw (pw:string) ->
        let nextModel = {
            currentModel with
                LoginModel = {currentModel.LoginModel with Password = pw}
        }
        nextModel, Cmd.none
    | _, UpdateRegisterModel (registerModel) ->
        let nextModel = {
            currentModel with
                RegisterModel = registerModel
        }
        nextModel, Cmd.none
    | _, UpdateExtraElement (element) ->
        let nextModel = {
            currentModel with
                ExtraReactElement = element
        }
        nextModel,Cmd.none
        ///functions to handle user registration
    | _, DotnetRegisterRequest (registermodel) ->
        let nextModel = { currentModel with ExtraReactElement = EmptyElement}
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetApi.dotnetRegister
                (registermodel)
                (Ok >> DotnetRegisterResponse)
                (Error >> DotnetRegisterResponse)
        nextModel,cmd
    | _, DotnetRegisterResponse (Ok value) ->
        let nextModel,cmd =
            match value with
            | RegisterFail x ->
                { currentModel with
                    Loading = false
                    ExtraReactElement = Message x
                    RegisterModel = {currentModel.RegisterModel with Password = ""}
                } , Cmd.none
            | RegisterSuccess x ->
                { currentModel with
                    Loading = false
                    ExtraReactElement = Message x
                }, Cmd.ofMsg DotnetGetUserRequest
        nextModel, cmd
    | _, DotnetRegisterResponse (Error e) ->
        let nextModel = {
            currentModel with
                Loading = false
                ErrorMsg = Some e.Message
                ExtraReactElement = Message e.Message
                RegisterModel = {currentModel.RegisterModel with Password = ""}
        }
        nextModel, Cmd.none
        /// functions to log in user via asp.net
    | _, DotnetLoginRequest (user) ->
        let cmdLogin =
            Cmd.OfAsync.either
                Server.dotnetApi.dotnetLogin
                user
                (Result.Ok >> DotnetLoginResponse)
                (Result.Error >> DotnetLoginResponse)
        let nextModel = {
            currentModel with
                Loading = true
            }
        nextModel,cmdLogin
    | _ , DotnetLoginResponse (Result.Error e) ->
        let nextModel = {
            currentModel with
                ErrorMsg = Some e.Message
                Loading = false
                LoginModel = {currentModel.LoginModel with Password = ""}
        }
        nextModel,Cmd.ofMsg (UpdateExtraElement (Message e.Message))
    | _ , DotnetLoginResponse (Result.Ok value) ->
        let (message,cmd) =
            match value with
            | LoginSuccess msg -> msg, Cmd.ofMsg DotnetGetUserRequest
            | LoginFail msg -> msg, Cmd.ofMsg (UpdateExtraElement (Message "Log In Failed"))
        let nextModel = {
            currentModel with
                ErrorMsg = Some message
                Loading = false
        }
        nextModel, cmd
        /// functions to access already logged in user information
    | _, DotnetGetUserRequest ->
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetSecureApi.dotnetGetUser
                ()
                (Result.Ok >> DotnetGetUserResponse)
                (Error >> DotnetGetUserResponse)
        currentModel, cmd
    | _, DotnetGetUserResponse (Ok value) ->
        let nextModel = {
            currentModel with
                //ExtraReactElement = Message "Getting User Information was successful"
                Authenticated = true
                User = Some value
                Loading = false
        }
        nextModel,Cmd.none
    | _, DotnetGetUserResponse (Error e) ->
        let nextModel = {
            currentModel with
                ExtraReactElement = Message "Getting User Information failed"
                Loading = false
                ErrorMsg = Some e.Message
        }
        nextModel,Cmd.none
        /// functions to access user-only counter
    | _, GetUserCounterRequest ->
        let nextModel = {
            currentModel with
                Loading = true
        }
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetSecureApi.getUserCounter
                ()
                (Ok >> GetUserCounterResponse)
                (Error >> GetUserCounterResponse)
        nextModel, cmd
    | _, GetUserCounterResponse (Ok value)->
        let nextModel = {
            currentModel with
                Loading = false
                Counter = Some value
        }
        nextModel, Cmd.none
    | _, GetUserCounterResponse (Error e)->
        let nextModel = {
            currentModel with
                Loading = false
                ErrorMsg = Some e.Message
        }
        nextModel, Cmd.ofMsg (UpdateExtraElement (Message "This function is for User only"))
        /// functions to handle user log out
    | _, DotnetLogOutRequest ->
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetSecureApi.dotnetUserLogOut
                ()
                (Ok >> DotnetLogOutResponse)
                (Error >> DotnetLogOutResponse)
        let nextModel = {
            currentModel with
                Loading = true
        }
        nextModel, cmd
    | _, DotnetLogOutResponse (Ok value) ->
        let (startModel,_) = init()
        let cmd =
            Cmd.OfAsync.perform
                Server.userApi.initialCounter
                ()
                InitialCountLoaded
        startModel, cmd
    | _, DotnetLogOutResponse (Error e) ->
        let nextModel = {
            currentModel with
                ErrorMsg = Some e.Message
        }
        nextModel, Cmd.none
    | _, Debug (message) ->
        { currentModel with ErrorMsg = Some message}, Cmd.none
    | _ -> currentModel, Cmd.none
