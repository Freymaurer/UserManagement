module Client.State

open System

open Elmish
open Shared

open Client.Types

// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = {
        Counter = None
        ErrorMsg = None
        InputString = ""
        User = None
        Loading = true
        Authenticated = false
        LoginModel = {Username = ""; Password = ""}
        RegisterModel = {Username = "";Password = "";Email = ""}
        ExtraReactElement = EmptyElement
        MainReactElement = Counter
        ShowMenuBool = false
        AdminUserList = [||]
        AdminUserListRoleFilter = None
        AdminViewUser = None
        AdminAssignRole = None
    }
    let loadCountCmd =
        Cmd.OfAsync.perform
                Server.userApi.initialCounter
                ()
                InitialCountLoaded
    let logInCmd =
        Cmd.OfAsync.perform
            Server.dotnetSecureApi.dotnetGetUser
            ()
            InitialUserLoaded
    let aggregrateCmds =
        Cmd.batch [loadCountCmd; logInCmd]
    initialModel, aggregrateCmds

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel.Counter, msg with
        // functions to manage counter
        ///
    | Some counter, Increment ->
        let nextModel = { currentModel with Counter = Some { Value = counter.Value + 1 } }
        nextModel, Cmd.none
        ///
    | Some counter, Decrement ->
        let nextModel = { currentModel with Counter = Some { Value = counter.Value - 1 } }
        nextModel, Cmd.none
        ///
    | _, UpdateInputString (str) ->
        let nextModel = { currentModel with InputString = str }
        nextModel,Cmd.none
        ///
    | _, InitialCountLoaded initialCount ->
        let nextModel = { currentModel with Counter = Some initialCount; Loading = false }
        nextModel, Cmd.none
        ///
    | _, InitialUserLoaded initialUser ->
        let nextModel =
            if initialUser.ExtLogin.IsTrue && initialUser.ExtLogin.IsUsernameSet = false
            then
                { currentModel with User = Some initialUser; Loading = false;Authenticated = false; ExtraReactElement = AddUsernameToExternLoginModal }
            else
                { currentModel with User = Some initialUser; Loading = false;Authenticated = true }
        nextModel, Cmd.none
        // Menu Management Functions
        ///
    | _, ClearRegisterLogin ->
        let nextModel = {
            currentModel with
                LoginModel = {Username = ""; Password = ""}
                RegisterModel = {Username = "";Password = "";Email = ""}
                AdminAssignRole = None
        }
        nextModel, Cmd.none
        ///
    | _, ToggleMenu ->
        let nextModel = {
            currentModel with ShowMenuBool = if currentModel.ShowMenuBool = true then false else true
        }
        nextModel,Cmd.none
        ///
    | _, ChangeMainReactElement (newElement) ->
        let nextModel = {
            currentModel with
                MainReactElement = newElement
                ShowMenuBool = false
        }
        nextModel, Cmd.none
        ///
    | _, SortAllUserList (searchString) ->
        let sortedArr =
            currentModel.AdminUserList
            |> Array.sortByDescending (
                fun x ->
                    let completeInfo = x.Username + " " + x.Email
                    Client.AuxFunctions.rankCompareStringsBySearchString searchString completeInfo
            )
        let nextModel = { currentModel with AdminUserList = sortedArr }
        nextModel,Cmd.none
        ///
    | _, FilterAllUserList (userRole) ->
        let nextModel = { currentModel with AdminUserListRoleFilter = userRole }
        nextModel,Cmd.none
        ///
    | _, AdminSelectUser (user) ->
        let nextModel = {
            currentModel with
                AdminViewUser = Some user
                MainReactElement = UserAccount user
        }
        nextModel,Cmd.none
        ///
    | _, AdminSelectAssignRole (role) ->
        let nextModel = { currentModel with AdminAssignRole = role }
        nextModel,Cmd.none
        // functions to manage input fields for user log in
        ///
    | _ , UpdateLoginUsername (name:string) ->
        let nextModel = {
            currentModel with LoginModel = {currentModel.LoginModel with Username = name}
        }
        nextModel, Cmd.none
        ///
    | _ , UpdateLoginUserPw (pw:string) ->
        let nextModel = {
            currentModel with LoginModel = {currentModel.LoginModel with Password = pw}
        }
        nextModel, Cmd.none
        ///
    | _, UpdateRegisterModel (registerModel) ->
        let nextModel = { currentModel with RegisterModel = registerModel }
        nextModel, Cmd.none
        ///
    | _, UpdateExtraElement (element) ->
        let nextModel = { currentModel with ExtraReactElement = element; InputString = "" }
        nextModel,Cmd.none
        // functions to handle user registration
        ///
    | _, DotnetRegisterRequest (registermodel) ->
        let nextModel = { currentModel with ExtraReactElement = EmptyElement }
        let cmd =
            Cmd.OfAsync.either
                Server.userApi.dotnetRegister
                (registermodel)
                (Ok >> DotnetRegisterResponse)
                (Error >> DotnetRegisterResponse)
        nextModel,cmd
        ///
    | _, DotnetRegisterResponse (Ok value) ->
        let nextModel,cmd =
            match value with
            | RegisterFail x ->
                {
                    currentModel with
                        Loading = false
                        ExtraReactElement = Message x
                        RegisterModel = {currentModel.RegisterModel with Password = ""}
                }
                , Cmd.none
            | RegisterSuccess ->
                { currentModel with Loading = false } , Cmd.ofMsg DotnetGetUserRequest
        nextModel, cmd
        ///
    | _, DotnetRegisterResponse (Error e) ->
        let nextModel = {
            currentModel with
                Loading = false
                ErrorMsg = Some e.Message
                ExtraReactElement = Message e.Message
                RegisterModel = {currentModel.RegisterModel with Password = ""}
        }
        nextModel, Cmd.none
        // functions to log in user via asp.net
        ///
    | _, DotnetLoginRequest (user) ->
        let nextModel = { currentModel with Loading = true }
        let cmdLogin =
            Cmd.OfAsync.either
                Server.userApi.dotnetLogin
                user
                (Result.Ok >> DotnetLoginResponse)
                (Result.Error >> DotnetLoginResponse)
        nextModel,cmdLogin
        ///
    | _ , DotnetLoginResponse (Result.Error e) ->
        let nextModel = {
            currentModel with
                ErrorMsg = Some e.Message
                Loading = false
                LoginModel = {currentModel.LoginModel with Password = ""}
                MainReactElement = Counter
        }
        nextModel,Cmd.ofMsg (UpdateExtraElement (Message e.Message))
        ///
    | _ , DotnetLoginResponse (Result.Ok value) ->
        let (nextModel,cmd) =
            match value with
            | LoginSuccess ->
                {
                    currentModel with
                        Loading = false
                        MainReactElement = Counter
                        LoginModel = {Username = ""; Password = ""}
                },
                Cmd.ofMsg DotnetGetUserRequest
            | LoginFail msg ->
                {
                    currentModel with
                        ErrorMsg = Some msg
                        Loading = false
                        MainReactElement = Counter
                        LoginModel = {Username = ""; Password = ""}
                },
                Cmd.ofMsg (UpdateExtraElement (Message msg))
        nextModel, cmd
        // functions to access already logged in user information
        ///
    | _, DotnetGetUserRequest ->
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetSecureApi.dotnetGetUser
                ()
                (Result.Ok >> DotnetGetUserResponse)
                (Error >> DotnetGetUserResponse)
        currentModel, cmd
        ///
    | _, DotnetGetUserResponse (Ok value) ->
        let nextModel =
            if value.ExtLogin.IsTrue && value.ExtLogin.IsUsernameSet = false
            then { currentModel with
                    Authenticated = true; User = Some value; Loading = false; ExtraReactElement = AddUsernameToExternLoginModal }
            else { currentModel with
                    Authenticated = true; User = Some value; Loading = false }
        nextModel,Cmd.none
        ///
    | _, DotnetGetUserResponse (Error e) ->
        let nextModel = {
            currentModel with
                ExtraReactElement = Message "Getting User Information failed"
                Loading = false
                ErrorMsg = Some e.Message
        }
        nextModel,Cmd.none
        // functions to access user-only counter
        ///
    | _, GetUserCounterRequest ->
        let nextModel = { currentModel with Loading = true }
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetSecureApi.getUserCounter
                ()
                (Ok >> GetUserCounterResponse)
                (Error >> GetUserCounterResponse)
        nextModel, cmd
        ///
    | _, GetUserCounterResponse (Ok value)->
        let nextModel = { currentModel with Loading = false; Counter = Some value }
        nextModel, Cmd.none
        ///
    | _, GetUserCounterResponse (Error e)->
        let nextModel = {
            currentModel with
                Loading = false
                ErrorMsg = Some e.Message
                ExtraReactElement = Message "This function is for User only"
        }
        nextModel, Cmd.none
        // functions to handle user log out
        ///
    | _, DotnetLogOutRequest ->
        let nextModel = { currentModel with Loading = true }
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetSecureApi.dotnetUserLogOut
                ()
                (Ok >> DotnetLogOutResponse)
                (Error >> DotnetLogOutResponse)
        nextModel, cmd
        ///
    | _, DotnetLogOutResponse (Ok value) ->
        let startModel,_ = init()
        let cmd =
            Cmd.OfAsync.perform
                Server.userApi.initialCounter
                ()
                InitialCountLoaded
        startModel, cmd
        ///
    | _, DotnetLogOutResponse (Error e) ->
        let nextModel = { currentModel with ErrorMsg = Some e.Message }
        nextModel, Cmd.none
        ///
    | _, DotnetChangeUserParamRequest (loginModel, userParam, input) ->
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetSecureApi.dotnetChangeUserParameters
                (loginModel, userParam, input)
                (Ok >> DotnetChangeUserParamResponse)
                (Error >> DotnetChangeUserParamResponse)
        let nextModel = {
            currentModel with
                Loading = true
        }
        nextModel, cmd
        ///
    | _, DotnetChangeUserParamResponse (Ok value) ->
        match value with
        | ChangeParamSuccess ->
            let nextModel = {
                currentModel with
                    Loading = false
                    LoginModel = {Username = ""; Password = ""}
                    MainReactElement = Counter
                }
            nextModel, Cmd.ofMsg DotnetGetUserRequest
        | ChangeParamFail str ->
            let nextModel = {
                currentModel with
                    Loading = false
                    ExtraReactElement = Message str
                    LoginModel = {currentModel.LoginModel with Password = ""}
                }
            nextModel, Cmd.none
        ///
    | _, DotnetChangeUserParamResponse (Error e) ->
        let nextModel = { currentModel with ExtraReactElement = Message e.Message }
        nextModel, Cmd.none
        ///
    | _, DeleteAccountRequest (loginModel) ->
        let nextModel = { currentModel with Loading = true }
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetSecureApi.dotnetDeleteUserAccount
                loginModel
                (Ok >> DeleteAccountResponse)
                (Error >> DeleteAccountResponse)
        nextModel, cmd
        ///
    | _, DeleteAccountResponse (Ok value) ->
        let initModel,_ = init()
        let initCmd =
            Cmd.OfAsync.perform
                Server.userApi.initialCounter
                ()
                InitialCountLoaded
        let nextModel,cmd =
            match value with
            | DeleteSuccess ->
                {initModel with ExtraReactElement = Message (sprintf "Account and all related information was deleted! If you were unhappy with our service please tell us about it at %s" ServiceHelpers.ServiceMail)}, initCmd
            | DeleteFail str ->
                Browser.Dom.window.alert ("You tried deleting the account and it failed. For security reasons you were logged out. " + str)
                currentModel, Cmd.ofMsg DotnetLogOutRequest
        nextModel, cmd
        ///
    | _, DeleteAccountResponse (Error e) ->
        let nextModel = {
            currentModel with
                ExtraReactElement = Message e.Message
                LoginModel = {Username = ""; Password = ""}
        }
        nextModel,Cmd.none
        ///
    | _, AddUsernameToExtLogin (username) ->
        let nextModel = { currentModel with Loading = true }
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetSecureApi.addUsernameToExtLogin
                (username)
                (Ok >> AddUsernameToExtLoginResponse)
                (Error >> AddUsernameToExtLoginResponse)
        nextModel,cmd
        ///
    | _, AddUsernameToExtLoginResponse (Ok value) ->
        match value with
        | ChangeParamSuccess ->
            init()
        | ChangeParamFail e ->
            Browser.Dom.window.alert (sprintf "Registering your new username failed! %s" e)
            init()
        ///
    | _, AddUsernameToExtLoginResponse (Error e) ->
        let nextModel = { currentModel with Loading = false; ExtraReactElement = Message e.Message}
        nextModel, Cmd.none
        ///
    | _, AdminGetAllUsersRequest ->
        let nextModel = { currentModel with Loading = true }
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetAdminSecureApi.dotnetGetAllUsers
                    ()
                    (Ok >> AdminGetAllUsersResponse)
                    (Error >> AdminGetAllUsersResponse)
        nextModel,cmd
        ///
    | _, AdminGetAllUsersResponse (Ok value) ->
        let nextModel = {
            currentModel with
                Loading = false
                AdminUserList = value
        }
        nextModel, Cmd.none
        ///
    | _, AdminGetAllUsersResponse (Error e) ->
        let nextModel = {
            currentModel with
                Loading = false
                ErrorMsg = Some e.Message
                ExtraReactElement = Message e.Message
        }
        nextModel, Cmd.none
        ///
    | _, AdminRegisterUserRequest (registermodel,role) ->
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetAdminSecureApi.adminRegisterUser
                    (registermodel,role)
                    (Ok >> AdminRegisterUserResponse)
                    (Error >> AdminRegisterUserResponse)
        let nextModel = {
            currentModel with
                Loading = true
                ExtraReactElement = EmptyElement
        }
        nextModel, cmd
        ///
    | _, AdminRegisterUserResponse (Ok value) ->
        let nextModel,cmd =
            match value with
            | RegisterFail x ->
                {
                    currentModel with
                        Loading = false
                        ExtraReactElement = Message x
                        RegisterModel = {currentModel.RegisterModel with Password = ""}
                }
                , Cmd.none
            | RegisterSuccess ->
                {
                    currentModel with
                        Loading = false
                }
                , Cmd.ofMsg AdminGetAllUsersRequest
        nextModel, cmd
        ///
    | _, AdminRegisterUserResponse (Error e) ->
        let nextModel = {
            currentModel with
                Loading = false
                ErrorMsg = Some e.Message
                ExtraReactElement = Message e.Message
                RegisterModel = {currentModel.RegisterModel with Password = ""}
        }
        nextModel, Cmd.none
        ///
    | _, AdminChangeUserParamsRequest (loginModel,user,userParam,input) ->
        let nextModel = { currentModel with Loading = true }
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetAdminSecureApi.adminChangeUserParameters
                (loginModel,user,userParam,input)
                (Ok >> AdminChangeUserParamsResponse)
                (Error >> AdminChangeUserParamsResponse)
        nextModel, cmd
        ///
    | _, AdminChangeUserParamsResponse (Ok value) ->
        match value with
        | ChangeParamSuccess ->
            let nextModel = {
                currentModel with
                    Loading = false
                    MainReactElement = UserList
                    ExtraReactElement = ExtraReactElement.EmptyElement
                    LoginModel = {Username = "";Password = ""}
                    InputString = ""
            }
            nextModel,Cmd.ofMsg AdminGetAllUsersRequest
        | ChangeParamFail str ->
            let nextModel = {
                currentModel with
                    Loading = false
                    ExtraReactElement = Message str
            }
            nextModel,Cmd.none
        ///
    | _, AdminChangeUserParamsResponse (Error e) ->
        let nextModel = {
            currentModel with
                Loading = false
                ExtraReactElement = Message e.Message
                InputString = ""
        }
        nextModel,Cmd.none
        ///
    | _, AdminDeleteAccountRequest (loginModel,user) ->
        let cmd =
            Cmd.OfAsync.either
                Server.dotnetAdminSecureApi.adminDeleteAccount
                (loginModel,user)
                (Ok >> AdminDeleteAccountResponse)
                (Error >> AdminDeleteAccountResponse)
        let nextModel = {
            currentModel with
                Loading = true
                LoginModel = {Username = "";Password = ""}
                ExtraReactElement = EmptyElement
        }
        nextModel,cmd
        ///
    | _, AdminDeleteAccountResponse (Ok value) ->
        let nextModel,cmd =
            match value with
            | DeleteSuccess ->
                {currentModel with MainReactElement = UserList}, Cmd.ofMsg AdminGetAllUsersRequest
            | DeleteFail str ->
                Browser.Dom.window.alert ("You tried deleting the account and it failed. For security reasons you were logged out. " + str)
                currentModel, Cmd.ofMsg DotnetLogOutRequest
        nextModel, cmd
        ///
    | _, AdminDeleteAccountResponse (Error e) ->
        let nextModel = {
            currentModel with
                ExtraReactElement = Message e.Message
                LoginModel = {Username = ""; Password = ""}
        }
        nextModel,Cmd.none
        // the following was used during development
        ///
    | _, GetContextClaimsRequest ->
        let cmd =
            Cmd.OfAsync.either
                Server.userApi.getContextClaims
                ()
                (Ok >> GetContextClaimsResponse)
                (Error >> GetContextClaimsResponse)
        currentModel, cmd
        ///
    | _, GetContextClaimsResponse (Ok value) ->
        let nextModel = {
            currentModel with ExtraReactElement = Message ("Connection was stable and succeded >=> " + value)
        }
        nextModel,Cmd.none
        ///
    | _, GetContextClaimsResponse (Error e) ->
        let nextModel = { currentModel with ExtraReactElement = Message e.Message }
        nextModel, Cmd.none
        ///
    | _, Debug (message) ->
        { currentModel with ErrorMsg = Some message}, Cmd.none
    | _ -> currentModel, Cmd.none