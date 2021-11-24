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
        | LoginRequest loginInfo ->
            let cmd =
                Cmd.OfAsync.either
                    Api.identityApi.login loginInfo
                    (LoginResponse >> IdentityMsg)
                    (curry GenericError Cmd.none)
            model, cmd
        | LoginResponse res ->
            let cmd =
                match res with
                | Error e   -> Browser.Dom.window.alert(e); Cmd.none
                | Ok ()     ->
                    let msg = IdentityMsg GetActiveUserRequest |> Cmd.ofMsg
                    let goToTodoPage = UpdatePage Route.Todo |> Cmd.ofMsg
                    Cmd.batch [msg; goToTodoPage]
            model, cmd
        | SignupRequest signupInfo ->
            let cmd =
                Cmd.OfAsync.either
                    Api.identityApi.register signupInfo
                    (SignupResponse >> IdentityMsg)
                    (curry GenericError Cmd.none)
            model, cmd
        | SignupResponse res ->
            let cmd =
                match res with
                | Error e   -> Browser.Dom.window.alert(e); Cmd.none
                | Ok ()     ->
                    let msg = IdentityMsg GetActiveUserRequest |> Cmd.ofMsg
                    let goToTodoPage = UpdatePage Route.Todo |> Cmd.ofMsg
                    Cmd.batch [msg; goToTodoPage]
            model, cmd
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
            }
            let cmd = UpdatePage Route.Todo |> Cmd.ofMsg
            nextModel, cmd
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

let updatePageHandler (model:Model) (page:Route) : Model * Cmd<Msg> =
    match page with
    | Route.Todo ->
        let s, cmd = Todo.init()
        let nextModel = {model with PageModel = PageModel.Todo s}
        nextModel, cmd
    | Route.Signup ->
        let s, cmd = Signup.init()
        let nextModel = {model with PageModel = PageModel.Signup s}
        nextModel, cmd
    | Route.Login ->
        let s, cmd = Login.init()
        let nextModel = {model with PageModel = PageModel.Login s}
        nextModel, cmd
    | Route.Settings ->
        let s, cmd = Settings.init()
        let nextModel = {model with PageModel = PageModel.Settings s}
        nextModel, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match model.PageModel, msg with
    | _, UpdatePage pm ->
        let nextModel, cmd = updatePageHandler model pm
        nextModel, cmd
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
    | PageModel.Todo state, TodoMsg msg ->
        let nextModel, cmd = Todo.update msg model state
        nextModel, cmd
    | PageModel.Login state, LoginMsg msg ->
        let nextModel, cmd = Login.update msg model state
        nextModel, cmd
    | PageModel.Signup state, SignupMsg msg ->
        let nextModel, cmd = Signup.update msg model state
        nextModel, cmd
    | PageModel.Settings state, SettingsMsg msg ->
        let nextModel, cmd = Settings.update msg model state
        nextModel, cmd
    | model, msg ->
        let text = $"Cannot handle ({model},{msg}) combination. Please check update logic."
        Browser.Dom.window.alert text
        failwith text
        
        