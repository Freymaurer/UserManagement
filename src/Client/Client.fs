module Client

open System

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Fulma
open Thoth.Json
open Fable.Core
open Shared

let [<Literal>] ENTER_KEY = 13.

type ExtraReactElement =
|EmptyElement
|RegisterModal
|Message of string

let onEnter msg dispatch =
    OnKeyDown (fun ev ->
        if ev.keyCode = ENTER_KEY then
            dispatch msg)


let emptyStr = str ""

let messageContainer (content:string) msg =
    Container.container [ ] [
        Columns.columns [ Columns.IsCentered ][
            Column.column [ Column.Width (Screen.All,Column.IsHalf) ][
                Content.content [
                    Content.Modifiers [Modifier.TextColor IsDanger;Modifier.TextAlignment (Screen.All,TextAlignment.Centered)]
                    Content.Props [
                        Style [
                            MarginTop "1%"; MarginBottom "2%"
                        ]
                    ]
                    ] [
                    str content
                ]
            ]
            Column.column [ Column.Width (Screen.All,Column.Is1) ][
                Delete.delete [ Delete.OnClick msg ][]
            ]
        ]
    ]

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = {
    Counter: Counter option
    ErrorMsg : string option
    LoginModel : LoginModel
    RegisterModel : RegisterModel
    User : User option
    Loading : bool
    Authenticated : bool
    ExtraReactElement : ExtraReactElement 
    }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
    | Increment
    | Decrement
    | InitialCountLoaded of Counter
    | UpdateLoginUsername of string
    | UpdateLoginUserPw of string
    | UpdateRegisterModel of RegisterModel
    | UpdateExtraElement of ExtraReactElement
    | Debug of string
    | GetTestRequest of string
    | GetTestResponse of Result<string, exn>
    | DotnetRegisterRequest of RegisterModel
    | DotnetRegisterResponse of Result<DotnetRegisterResults,exn>
    | DotnetLoginRequest of LoginModel
    | DotnetLoginResponse of Result<DotnetLoginResults,exn>
    | DotnetGetUserRequest
    | DotnetGetUserResponse of Result<User,exn>
    | DotnetLogOutRequest
    | DotnetLogOutResponse of Result<DotnetLogOutResults,exn>
    | GetUserCounterRequest
    | GetUserCounterResponse of Result<Counter,exn>

module ServerPath =
    open System
    open Fable.Core

    /// when publishing to IIS, your application most likely runs inside a virtual path (i.e. localhost/SafeApp)
    /// every request made to the server will have to account for this virtual path
    /// so we get the virtual path from the location
    /// `virtualPath` of `http://localhost/SafeApp` -> `/SafeApp/`
    [<Emit("window.location.pathname")>]
    let virtualPath : string = jsNative

    /// takes path segments and combines them into a valid path
    let combine (paths: string list) =
        paths
        |> List.map (fun path -> List.ofArray (path.Split('/')))
        |> List.concat
        |> List.filter (fun segment -> not (segment.Contains(".")))
        |> List.filter (String.IsNullOrWhiteSpace >> not)
        |> String.concat "/"
        |> sprintf "/%s"

    /// Normalized the path taking into account the virtual path of the server
    let normalize (path: string) = combine [virtualPath; path]

module Server =

    open Shared
    open Fable.Remoting.Client

    // normalize routes so that they work with IIS virtual path in production
    let normalizeRoutes typeName methodName =
        Route.builder typeName methodName
        |> ServerPath.normalize

    /// A proxy you can use to talk to server directly
    let userApi : ICounterApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder normalizeRoutes
        |> Remoting.buildProxy<ICounterApi>

    let dotnetApi : IDotnetApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder normalizeRoutes
        |> Remoting.buildProxy<IDotnetApi>

    let dotnetSecureApi : IDotnetSecureApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder normalizeRoutes
        |> Remoting.buildProxy<IDotnetSecureApi>

let myDecode64 (str64:string) =
    let l = str64.Length
    let padNum = l%4
    let padding = if padNum = 0 then "" else Array.init (4-padNum) (fun _ -> "=") |> String.concat ""
    let newStr = str64 + padding
    let toByteArr = System.Convert.FromBase64String(newStr)
    System.Text.Encoding.UTF8.GetString (toByteArr)

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
    }
    let loadCountCmd =
        Cmd.OfAsync.perform
                Server.userApi.initialCounter
                ()
                InitialCountLoaded
    initialModel, loadCountCmd

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
                ExtraReactElement = Message "Getting User Information was successful"
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
        init()
    | _, DotnetLogOutResponse (Error e) ->
        let nextModel = {
            currentModel with
                ErrorMsg = Some e.Message
        }
        nextModel, Cmd.none
    | _, Debug (message) ->
        { currentModel with ErrorMsg = Some message}, Cmd.none
    | _ -> currentModel, Cmd.none

let inputRegisterBox inputElement header valueOrDefault msg =
    Box.box' [ Props [ Class "registerBox" ] ] [
        Text.div [ Props [ Style [ PaddingLeft "1rem" ] ] ] [
            str header
        ]
        inputElement [
            Input.OnChange msg
                //(fun e ->
                //    let newModel = {model.RegisterModel with Username = e.Value}
                //    dispatch (UpdateRegisterModel newModel)
                //    )
            Input.ValueOrDefault valueOrDefault
            Input.Props [
                Style [
                    BoxShadow "none"; Border "none";
                    BackgroundColor "#f2f2f2"
                ]
            ]
        ]
    ]

let registerModal (model : Model) (dispatch : Msg -> unit) =
    Modal.modal [
        Modal.IsActive true
        ] [
        Modal.background [ Props [OnClick (fun _ -> dispatch (UpdateExtraElement EmptyElement) )] ] [ ]
        Modal.Card.card [
            Modifiers [ Modifier.BackgroundColor IsWhite ]
            Props [ Style [ Height "80%";BorderRadius "15px" ] ]
            ] [
            Modal.Card.head [
                Modifiers [Modifier.BackgroundColor IsWhite]
                Props [ Style [ BorderBottom "0px"] ]
                ] [
                Modal.Card.title [ Props [ Style [ PaddingTop "2rem" ] ] ] [
                    str "Create your account"
                ]
            ]
            Modal.Card.body
                [ ] [
                inputRegisterBox
                    Input.text
                    "Username"
                    model.RegisterModel.Username
                    (fun e ->
                        let newModel = {model.RegisterModel with Username = e.Value}
                        dispatch (UpdateRegisterModel newModel)
                        )
                inputRegisterBox
                    Input.email
                    "Email"
                    model.RegisterModel.Email
                    (fun e ->
                        let newModel = {model.RegisterModel with Email = e.Value}
                        dispatch (UpdateRegisterModel newModel)
                        )
                inputRegisterBox
                    Input.password
                    "Password"
                    model.RegisterModel.Password
                    (fun e ->
                        let newModel = {model.RegisterModel with Password = e.Value}
                        dispatch (UpdateRegisterModel newModel)
                        )
                Columns.columns [ Columns.Props [ Style [ PaddingTop "2rem" ] ] ][
                    Column.column [Column.Offset (Screen.All,Column.IsThreeFifths)] [
                        Button.button [
                            Button.Color IsInfo
                            (if model.RegisterModel.Username = "" || model.RegisterModel.Password = "" || model.RegisterModel.Email = "" then Button.Disabled true else Button.Disabled false )
                            Button.OnClick (fun _ -> dispatch (DotnetRegisterRequest model.RegisterModel))
                            ][
                            str "Register"
                        ]
                    ]
                ]
                ]
            ] 
        ]

let safeComponents =
    let components =
        span [ ]
           [ a [ Href "https://github.com/SAFE-Stack/SAFE-template" ]
               [ str "SAFE  "
                 str Version.template ]
             str ", "
             a [ Href "https://saturnframework.github.io" ] [ str "Saturn" ]
             str ", "
             a [ Href "http://fable.io" ] [ str "Fable" ]
             str ", "
             a [ Href "https://elmish.github.io" ] [ str "Elmish" ]
             str ", "
             a [ Href "https://fulma.github.io/Fulma" ] [ str "Fulma" ]
             str ", "
             a [ Href "https://zaid-ajaj.github.io/Fable.Remoting/" ] [ str "Fable.Remoting" ]
           ]

    span [ ]
        [ str "Version "
          strong [ ] [ str Version.app ]
          str " powered by: "
          components ]

let debug (m:Model) =
    match m with
    | { ErrorMsg = Some value } -> string m.ErrorMsg
    | { ErrorMsg = None } -> ""

let show model =
    match model with
    | { Counter = Some counter;Loading = false } -> string counter.Value
    | _ -> "Loading..."

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let loginNavbar (model : Model) (dispatch : Msg -> unit) = [
    Navbar.Item.div [ ] [ 
        Heading.h2 [ ] [ str "SAFE Template - Login" ]
    ]
    Navbar.End.div [ ] [
        Navbar.Item.a [
            Navbar.Item.IsHoverable;
            Navbar.Item.HasDropdown;
            Navbar.Item.Props [Style [MarginRight "2rem"] ]
        ] [
            Navbar.Link.a [ ] [
                Text.div
                    [ Modifiers [ Modifier.TextWeight TextWeight.SemiBold; Modifier.TextColor Color.IsWhiteBis ] ]
                    [ str "Log In"]
            ]
            Navbar.Dropdown.div [
                Navbar.Dropdown.IsRight
                Navbar.Dropdown.Props [ Style [ Width "15rem" ] ]
                ] [
                Navbar.Item.div
                    [ Navbar.Item.Props [Style [Cursor "text"]];Navbar.Item.Modifiers [Modifier.TextColor IsGrey] ]
                    [ str "Have an account?" ]
                Navbar.Item.div [ ] [
                    Input.text
                        [ Input.OnChange (
                            fun e ->
                                //let newUser = { model.User with Username = e.Value}
                                dispatch (UpdateLoginUsername e.Value)
                            )
                          Input.Placeholder "Username"
                          Input.Props [ Id "UserName" ]
                            ]
                    ]
                Navbar.Item.div [ ][
                    Input.password
                        [ Input.OnChange (
                            fun e ->
                                //let newUser = { model.User with Password = e.Value}
                                dispatch (UpdateLoginUserPw e.Value)
                            )
                          Input.Placeholder "Password"
                          Input.Props [ Id "UserPw" ]
                            ]
                    ]
                Navbar.Item.a [
                    Navbar.Item.Props [
                        OnClick (fun _ -> dispatch (DotnetLoginRequest model.LoginModel));
                        Style [
                            PaddingLeft "5%" ; PaddingRight "5%";
                            AlignContent AlignContentOptions.Center;
                            BorderRadius "10px"
                            MarginLeft "5%"; MarginRight "5%"; MarginTop "3%"
                        ]
                    ]
                    Navbar.Item.Modifiers [ Modifier.BackgroundColor IsInfo; Modifier.TextColor IsWhite ]
                    ] [
                    Text.p
                        [ Modifiers [ Modifier.TextAlignment (Screen.All,TextAlignment.Centered) ]; Props [ Style [ TextAlign TextAlignOptions.Center; Width "90%" ] ] ]
                        [ str "Login" ]
                    ]
                Navbar.divider [] []
                Navbar.Item.div
                    [ Navbar.Item.Props [Style [Cursor "text"]];Navbar.Item.Modifiers [Modifier.TextColor IsGrey] ]
                    [ str "New here?" ]
                Navbar.Item.a [
                    Navbar.Item.Props [
                        OnClick (fun _ -> dispatch (UpdateExtraElement RegisterModal));
                        Style [
                            PaddingLeft "5%" ; PaddingRight "5%";
                            AlignContent AlignContentOptions.Center;
                            BorderRadius "10px"
                            MarginLeft "5%"; MarginRight "5%"; MarginTop "3%"; MarginBottom "7%"
                            Border @"1px solid hsl(204, 86%, 53%)"
                        ]
                    ]
                    Navbar.Item.Modifiers [Modifier.TextColor IsInfo ]
                    ] [ 
                    Text.p [
                        Modifiers [ Modifier.TextAlignment (Screen.All,TextAlignment.Centered) ];
                        Props [ Style [ TextAlign TextAlignOptions.Center; Width "90%" ] ]
                        ]
                        [ str "Sign Up" ]
                    ]
                ]
        ]
    ]
    ]

let loggedInNavbar (model : Model) (dispatch : Msg -> unit) =
    [
        Navbar.Item.div
            [ ]
            [ Heading.h2 [ ]
                [ str "SAFE Template - Login" ]
            ]
        Navbar.End.a [ ] [
            Navbar.Item.div [
                Navbar.Item.IsHoverable;
                Navbar.Item.HasDropdown;
            ] [
                Navbar.Link.a [ Navbar.Link.IsArrowless ] [
                    Text.span
                        [ Modifiers [ Modifier.TextWeight TextWeight.SemiBold; Modifier.TextColor Color.IsWhiteBis ] ]
                        [ str (if model.User.IsSome then model.User.Value.Username else "No User Information")]
                ]
                Navbar.Dropdown.div [ Navbar.Dropdown.IsRight ] [
                    Navbar.divider [ ] [ ]
                    Navbar.Item.a
                        [ Navbar.Item.Props [OnClick ( fun _ -> dispatch DotnetLogOutRequest)] ]
                        [ str "Logout" ]
                ]
            ]
            Navbar.Item.div [] [br []]
        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    let extraEle =
        match model.ExtraReactElement with
        |EmptyElement -> emptyStr
        |RegisterModal -> registerModal model dispatch
        |Message x -> messageContainer x (fun _ -> dispatch (UpdateExtraElement EmptyElement))
    div [ Style [  ] ]
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            (if model.Authenticated = true then (loggedInNavbar model dispatch) else (loginNavbar model dispatch ))
          br []
          Container.container []
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ Heading.h3 [] [ str ("Press buttons to manipulate counter: " + show model) ] ]
                extraEle
                Columns.columns []
                    [ Column.column [] [ button "-" (fun _ -> dispatch Decrement) ]
                      Column.column [] [ button "+" (fun _ -> dispatch Increment) ]
                      Column.column [] [ button "secret" (fun _ -> dispatch GetUserCounterRequest) ] ] ]
          str (Browser.Dom.document.cookie)
          Footer.footer [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ safeComponents ] ] ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run