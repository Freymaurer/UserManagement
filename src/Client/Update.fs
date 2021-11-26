module Update

open Elmish

open Shared
open Model
open Messages

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
        | LoginResponse () ->
            let cmd =
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
        | SignupResponse() ->
            let cmd =
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
            Browser.Dom.window.location.reload()
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
            let userState : UserState = { UserState.init with LoggedIn = true; User = Some user} 
            let nextModel = { model with UserState = userState }
            nextModel, Cmd.none
        | UpdateUserProfileRequest newUserInfo ->
            let cmd =
                Cmd.OfAsync.either
                    Api.userApi.updateUserProfile
                    newUserInfo
                    (UpdateUserProfileResponse >> IdentityMsg)
                    (curry GenericError Cmd.none)
            model, cmd
        | UpdateUserProfileResponse user ->
            let nextModel = { model with UserState = { model.UserState with User = Some user } }
            let cmd = UpdatePage Route.Profile |> Cmd.ofMsg
            nextModel, cmd
        | UpdateUserPasswordRequest (login,newPw) ->
            let cmd =
                Cmd.OfAsync.either
                    Api.userApi.updatePassword
                    (login,newPw)
                    (UpdateUserPasswordResponse >> IdentityMsg)
                    (curry GenericError Cmd.none)
            model, cmd
        | UpdateUserPasswordResponse() ->
            let nextModel = {
                model with
                    UserState = { model.UserState with PasswordModalPw = ""; ShowPasswordModal = None }
            }
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
    | Route.Profile ->
        /// Hydrate model with user data, to effectively change all params on update
        /// This might be necessary to restructure if user profile contains more and more information
        let s, cmd =
            if model.UserState.LoggedIn && model.UserState.User.IsSome then
                Profile.init(model.UserState.User)
            else
                Profile.init(None)
        let nextModel = {model with PageModel = PageModel.Profile s}
        nextModel, cmd
    | Route.AuthTest ->
        let s, cmd = AuthTest.init()
        let nextModel = {model with PageModel = PageModel.AuthTest s}
        nextModel, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match model.PageModel, msg with
    | _, UpdatePage pm ->
        let nextModel, cmd = updatePageHandler model pm
        nextModel, cmd
    | _, UpdateNavbarMenuState b ->
        let nextModel = {model with NavbarMenuState = b}
        nextModel, Cmd.none
    | _, UpdatePasswordModal pipeIntoMsg ->
        let nextModel = {model with UserState = { model.UserState with ShowPasswordModal = pipeIntoMsg; PasswordModalPw = ""} }
        nextModel, Cmd.none
    | _, UpdatePasswordModalPw confirmPw ->
        let nextModel = {model with UserState = { model.UserState with PasswordModalPw = confirmPw} }
        nextModel, Cmd.none
    | _, GenericError (nextCmd,e) ->
        let alertMsg =
            try 
                e.GetPropagatedError()
            with
                | exn -> e.Message
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
    | PageModel.Profile state, ProfileMsg msg ->
        let nextModel, cmd = Profile.update msg model state
        nextModel, cmd
    | PageModel.AuthTest state, AuthTestMsg msg ->
        let nextModel, cmd = AuthTest.update msg model state
        nextModel, cmd
    | model, msg ->
        let text = $"Cannot handle ({model},{msg}) combination. Please check update logic."
        //Browser.Dom.window.alert text
        failwith text
        
        