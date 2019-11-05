module Client.View

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
open Client.State

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
    Navbar.Item.div [ Navbar.Item.Props [ Style [ MarginLeft "1rem"; MarginRight "0.5rem" ] ] ] [
        Fa.i [
            Fa.Solid.Bars
            Fa.Props [
                OnClick (fun _ ->  dispatch ToggleMenu)
                Style [
                    Cursor "pointer"
                ]
            ]
        ] []
    ]
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
                          Input.Props [ Id "UserName"; onEnter (DotnetLoginRequest model.LoginModel) dispatch ]
                            ]
                    ]
                Navbar.Item.div [ ][
                    Input.password
                        [ Input.OnChange (
                            fun e ->
                                dispatch (UpdateLoginUserPw e.Value)
                            )
                          Input.Placeholder "Password"
                          Input.Props [ Id "UserPw"; onEnter (DotnetLoginRequest model.LoginModel) dispatch ]
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
        Navbar.Item.div [ Navbar.Item.Props [ Style [ MarginLeft "1rem"; MarginRight "0.5rem" ] ] ] [
            Fa.i [
                Fa.Solid.Bars
                Fa.Props [
                    OnClick (fun _ ->  dispatch ToggleMenu)
                    Style [
                        Cursor "pointer"
                    ]
                ]
            ] []
        ]
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

let extraEle model dispatch =
    match model.ExtraReactElement with
    |EmptyElement -> emptyStr
    |RegisterModal -> registerModal model dispatch
    |Message x -> messageContainer x (fun _ -> dispatch (UpdateExtraElement EmptyElement))

let counter model dispatch = 
      Container.container [ Container.Props [ Style [MarginTop "1rem"] ] ]
          [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ Heading.h3 [] [ str ("Press buttons to manipulate counter: " + show model) ] ]
            extraEle model dispatch
            Columns.columns []
                [ Column.column [] [ button "-" (fun _ -> dispatch Decrement) ]
                  Column.column [] [ button "+" (fun _ -> dispatch Increment) ]
                  Column.column [] [ button "secret" (fun _ -> dispatch GetUserCounterRequest) ] ] ]

// Helper to generate a menu item
let menuItem label msg =
    Menu.Item.li [ Menu.Item.OnClick msg]
        [ str label ]

// Helper to generate a sub menu
let subMenu label  children =
    li [ ] [
        Menu.Item.a [ ]
           [ str label ]
        ul [ ] children
    ]

let menu (model:Model) dispatch =
    let unAuthenticated =
        div [
            Style [BackgroundColor "white";PaddingTop "1rem";PaddingLeft "1rem";Height "100%";]
        ][
            str "This site is meant for account management. Please log in to use the provided functions."
        ]
    let authenticated =
        Menu.menu [
            Modifiers [ Modifier.TextSize (Screen.All,TextSize.Is7)]
            Props [Style [BackgroundColor "white";PaddingTop "1rem";PaddingLeft "1rem";Height "100%"]]
        ] [
            Menu.label [ ] [ str "User Account" ]
            Menu.list [ ]
              [ menuItem "Settings" (fun _ -> dispatch (ChangeMainReactElement UserAccount)) ]
            Menu.label [ ] [ str "Account Management" ]
            Menu.list [ ]
              [ menuItem "User List"  (fun _ -> dispatch (ChangeMainReactElement UserList))
                subMenu "Role Rights"
                      [ menuItem "Members" (fun _ -> dispatch (ChangeMainReactElement (RoleRights User)))
                        menuItem "User Manager" (fun _ -> dispatch (ChangeMainReactElement (RoleRights UserManager)))
                        menuItem "Admin" (fun _ -> dispatch (ChangeMainReactElement (RoleRights Admin))) ] ] 
            Menu.label [] [str "Debug"]
            Menu.list []
                [menuItem "Test Counter" (fun _ -> dispatch (ChangeMainReactElement Counter))]
        ]
    match model.Authenticated with
    | true -> authenticated
    | false -> unAuthenticated

let menuCard model dispatch =
    div [
        Style [
            Position PositionOptions.Absolute;
            BackgroundColor "rgba(0, 0, 0, 0.5)"
            ZIndex "100"
            Width "100%" 
            Height "100%"
            Transition "Visibility"
            TransitionDuration (if model.ShowMenuBool = true then "0s" else ".50s")
            Visibility (if model.ShowMenuBool = true then "visible" else "hidden")
        ]
    ][
        /// just a background element that will toggle off the menu when clicked on
        div [OnClick (fun _ -> dispatch ToggleMenu);Style [Width "100%";Height "100%";Position PositionOptions.Absolute;ZIndex "200"]][]
        /// The menu itself
        Column.column [
            Column.Width (Screen.All,Column.Is2);
            Column.Modifiers [ Modifier.IsPaddingless ];
            Column.Props [
                Style [
                    Height "100%";ZIndex "300"; Position PositionOptions.Absolute
                    Transform (if model.ShowMenuBool = true then "translate3d(0, 0, 0)" else "translate3d(-100%, 0, 0)")
                    TransitionDuration ".50s"
                    TransitionProperty "transform"
                ]
            ]
        ] [
            Navbar.navbar [ Navbar.Color IsBlack ] [
                Navbar.Item.div [Navbar.Item.Props [ Style [ MarginLeft "1rem"; MarginRight "0.5rem" ;MinHeight "3.25rem"] ]][
                    Fa.i [
                        Fa.Solid.Bars
                        Fa.Props [
                            OnClick (fun _ ->  dispatch ToggleMenu)
                            Style [
                                Cursor "pointer"
                            ]
                        ]
                    ] [ ]
                ]
            ]
            menu model dispatch
        ]
    ]

let constructionLabel model dispatch =
    Column.column [ Column.Width (Screen.All,Column.IsHalf); Column.Offset (Screen.All, Column.IsOneQuarter);][
        Box.box' [ Modifiers [Modifier.TextColor IsWarning; Modifier.BackgroundColor Color.IsWhiteTer] ][
            p [Style [TextAlign TextAlignOptions.Center;FontWeight "bold"]][str "ATTENTION YOU ENTERED A LINK THAT IS STILL UNDER CONSTRUCTION!"]
            p [Style [TextAlign TextAlignOptions.Center;FontWeight "bold"]][str "PLEASE COME AGAIN LATER!"]
            div [ Style [Width "100%";MarginTop "2rem"; Display DisplayOptions.Flex; JustifyContent "center" ] ][
                Fa.span [ Fa.Solid.Wrench; Fa.Size Fa.Fa6x;Fa.Props [Style[MarginLeft "1rem";MarginRight "1rem"]]][]
                Fa.span [ Fa.Solid.Tools; Fa.Size Fa.Fa6x;Fa.Props [Style[MarginLeft "1rem";MarginRight "1rem"]]][]
                Fa.span [ Fa.Solid.PaintRoller; Fa.Size Fa.Fa6x;Fa.Props [Style[MarginLeft "1rem";MarginRight "1rem"]] ][]
            ]
        ]
    ]

/// View for User Account Information

let userAccountElement model dispatch =
    str "hello"
    