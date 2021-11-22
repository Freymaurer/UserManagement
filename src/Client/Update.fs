module Update

open Elmish

open Shared
open Model
open Messages

let curry f a b = f (a,b)

module Identity =

    open Identity
    // reopen Messages to set priority to these
    open Messages

    let update (identityMsg:Identity.Msg) (model: Model) : Model * Cmd<Msg> =
        match identityMsg with
        | LoginRequest loginModel ->
            let cmd =
                Cmd.OfAsync.either
                    Api.identityApi.login loginModel
                    (LoginResponse >> IdentityMsg)
                    (curry GenericError Cmd.none )
            model, cmd
        | LoginResponse res ->
            let nextModel = 
                match res with
                | Error e   -> Browser.Dom.window.alert(e); model
                | Ok ()     -> {model with PageModel = PageModel.Home Todo.Model.init}
            nextModel, IdentityMsg GetActiveUserRequest |> Cmd.ofMsg
        | LogoutRequest ->
            let cmd =
                Cmd.OfAsync.either
                    Api.userApi.logout
                    ()
                    (LogoutResponse >> IdentityMsg)
                    (curry GenericError Cmd.none)
            model, cmd
        | LogoutResponse() ->
            let nextModel = {
                model with
                    UserState = UserState.init;
                    PageModel = PageModel.Home Todo.Model.init
            }
            nextModel, Cmd.none
        | GetActiveUserRequest ->
            let cmd =
                Cmd.OfAsync.either
                    Api.userApi.getActiveUser
                    ()
                    (GetActiveUserResponse >> IdentityMsg)
                    (curry GenericError Cmd.none)
            model, cmd
        | GetActiveUserResponse user ->
            let userState : UserState = {LoggedIn = true; User = Some user}
            let nextModel = { model with UserState = userState }
            nextModel, Cmd.none
        | GetNumRequest ->
            let cmd =
                Cmd.OfAsync.perform
                    Api.identityApi.getNumTest
                    ()
                    (fun x -> GenericLog $"{x}")
            model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match model.PageModel, msg with
    | _, UpdatePageModel pm ->
        let nextModel = {
            model with
                PageModel = pm
        }
        nextModel, Cmd.none
    | _, UpdateNavbarMenuState b ->
        let nextModel = {model with NavbarMenuState = b}
        nextModel, Cmd.none
    | _, GenericError (nextCmd,e) ->
        let alertMsg = $"{e.GetPropagatedError()}"
        Browser.Dom.window.alert(alertMsg)
        model, nextCmd
    | _, GenericLog str ->
        Browser.Dom.window.alert(str)
        model, Cmd.none
    | _, IdentityMsg msg ->
        let nextModel, cmd = Identity.update msg model
        nextModel, cmd
    | PageModel.Home todoModel, TodoMsg msg ->
        let nextModel, cmd = Todo.update msg model todoModel
        nextModel, cmd
    | PageModel.Login loginModel, LoginMsg msg ->
        let nextModel, cmd = Login.update msg model loginModel
        nextModel, cmd
        
        