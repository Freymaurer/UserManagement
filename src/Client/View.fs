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
    Container.container [ Container.Props [Style [MarginTop "2%"]]] [
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
        Modal.Props [onEnter (DotnetRegisterRequest model.RegisterModel) dispatch]
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
            Modal.Card.body  [ ] [
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
                Columns.columns [ Columns.Props [ Style [ PaddingTop "2rem" ] ] ] [
                    Column.column [Column.Offset (Screen.All,Column.IsThreeQuarters)] [
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

let verifyLoginModal (model : Model) extraElement (dispatch : Msg -> unit) msgInput =
    let msg =
        match msgInput with
        | DeleteAccountRequest _ -> DeleteAccountRequest model.LoginModel
        | AdminDeleteAccountRequest _ -> AdminDeleteAccountRequest (model.LoginModel,model.AdminViewUser.Value)
        | DotnetChangeUserParamRequest (_,userParam,_) -> DotnetChangeUserParamRequest (model.LoginModel,userParam,model.InputString)
        | AdminChangeUserParamsRequest (_,_,userParam,_) -> AdminChangeUserParamsRequest (model.LoginModel,model.AdminViewUser.Value,userParam,model.InputString)
        | _ -> UpdateExtraElement (Message "There went something wrong! This should never happen")
    Modal.modal [
        Modal.IsActive true
        Modal.Props [onEnter msg dispatch]
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
                Modal.Card.title [ Props [ Style [ PaddingTop "1.5rem" ] ] ] [
                    str "Verify your Login"
                ]
            ]
            Modal.Card.body  [ ] [
                extraElement
                inputRegisterBox
                    Input.text
                    "Current Username"
                    model.LoginModel.Username
                    (fun e -> dispatch (UpdateLoginUsername e.Value))
                inputRegisterBox
                    Input.password
                    "Current Password"
                    model.LoginModel.Password
                    (fun e -> dispatch (UpdateLoginUserPw e.Value))
                Columns.columns [ Columns.Props [ Style [ PaddingTop "2rem" ] ] ] [
                    Column.column [Column.Offset (Screen.All,Column.IsHalf)] [
                        Button.button [
                            Button.Color IsDanger
                            (if model.LoginModel.Username = "" || model.LoginModel.Password = "" then Button.Disabled true else Button.Disabled false )
                            Button.OnClick (fun _ -> dispatch msg)
                        ][
                            str "Verify your login"
                        ]
                    ]
                ]
            ]
        ] 
    ]

let adminRegisterModal model dispatch =
    let adminView =
        Column.column [][
            Dropdown.dropdown [ Dropdown.IsHoverable;Dropdown.IsUp] [
                div [ ] [
                    Button.button [ Button.Modifiers [Modifier.BackgroundColor IsWhiteTer] ] [
                        span [ ] [ str (if model.AdminAssignRole = Guest then "Role" else string model.AdminAssignRole) ]
                        Icon.icon [ Icon.Size IsSmall ] [ Fa.i [ Fa.Solid.AngleDown ] [ ] ]
                    ]
                ]
                Dropdown.menu [ ] [
                    Dropdown.content [ ] [
                        Dropdown.Item.a [ Dropdown.Item.Props [OnClick (fun _ -> dispatch (AdminSelectAssignRole ActiveUserRoles.Admin)) ] ] [ str "Admin" ]
                        Dropdown.Item.a [ Dropdown.Item.Props [OnClick (fun _ -> dispatch (AdminSelectAssignRole ActiveUserRoles.UserManager)) ] ] [ str "UserManager" ]
                        Dropdown.divider [ ]
                        Dropdown.Item.a [ Dropdown.Item.Props [OnClick (fun _ -> dispatch (AdminSelectAssignRole ActiveUserRoles.User)) ] ] [ str "User" ]
                    ]
                ]
            ]
        ]
    Modal.modal [
        Modal.IsActive true
        Modal.Props [onEnter (AdminRegisterUserRequest (model.RegisterModel,model.AdminAssignRole)) dispatch]
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
            Modal.Card.body[ ] [
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
                Columns.columns [ Columns.Props [ Style [ PaddingTop "2rem" ] ] ] [
                    adminView
                    Column.column [] [
                        Button.button [
                            Button.Color IsInfo
                            (if model.RegisterModel.Username = "" || model.RegisterModel.Password = "" || model.RegisterModel.Email = "" then Button.Disabled true else Button.Disabled false )
                            Button.OnClick (fun _ -> dispatch (AdminRegisterUserRequest (model.RegisterModel,model.AdminAssignRole)))
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
    Navbar.End.a [ ] [
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

let counter model dispatch = 
      Container.container [ Container.Props [ Style [MarginTop "1rem"] ] ]
          [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ Heading.h3 [] [ str ("Press buttons to manipulate counter: " + show model) ] ]
            Columns.columns []
                [ Column.column [] [ button "-" (fun _ -> dispatch Decrement) ]
                  Column.column [] [ button "+" (fun _ -> dispatch Increment) ]
                  Column.column [] [ button "secret" (fun _ -> dispatch GetUserCounterRequest) ] ] ]

// Helper to generate a menu item
let menuItem label msg =
    Menu.Item.li [ Menu.Item.OnClick msg]
        [ str label ]

// Helper to generate a sub menu
let subMenu label children =
    li [ ] [
        Menu.Item.a [ ]
           [ str label ]
        ul [ ] children
    ]

let menu (model:Model) dispatch =
    let hideElementsBy threshold=
        Screen.All, if AuxFunctions.authentificationLevelByUser model.User >= threshold then false else true
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
              [ menuItem "Account Information" (fun _ -> dispatch (ChangeMainReactElement (UserAccount model.User.Value))) ]
            Menu.label [ Modifiers [Modifier.IsHidden (hideElementsBy 5)] ] [ str "Account Management" ]
            Menu.list [ Modifiers [Modifier.IsHidden (hideElementsBy 5)] ] [
                menuItem "User List"  (fun _ ->
                dispatch AdminGetAllUsersRequest
                dispatch (ChangeMainReactElement UserList))
            ]
            Menu.label [ Modifiers [Modifier.IsHidden (hideElementsBy 10)] ] [str "Debug"]
            Menu.list [Modifiers [Modifier.IsHidden (hideElementsBy 10)]]
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
                Navbar.Item.div [Navbar.Item.Props [ Style [ MarginLeft "1rem"; MarginRight "0.5rem" ;MinHeight "3.25rem"] ]][
                    div [ Style[Color "#e6e6e6"] ][str (if model.User.IsSome then model.User.Value.Username else "Log In")]
                ]
            ]
            menu model dispatch
        ]
    ]

let constructionLabel model dispatch =
    Column.column [ Column.Width (Screen.All,Column.IsHalf); Column.Offset (Screen.All, Column.IsOneQuarter);][
        Box.box' [ Modifiers [Modifier.BackgroundColor Color.IsWhiteTer]; Props[Style[Color "#ff9900"]] ][
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

let userAccountinformationColumn headerStr informationStr msg =
    Columns.columns [][
        Column.column [][
            Heading.h5 [][str headerStr]
            Heading.h6 [Heading.IsSubtitle][str informationStr]
        ]
        Column.column [ Column.Modifiers [Modifier.TextAlignment (Screen.All,TextAlignment.Right)] ][
            Button.a [ Button.OnClick msg ][str "Change"]
        ]
    ]

let inputUsername dispatch =
    Container.container [][
        text [][str "Type in your new username and verify your login data."]
        Column.column [][
            Input.text [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Username"]
        ]
    ]

let inputEmail dispatch =
    Container.container [][
        text [][str "Type in your new Email adress and verify your login data."]
        Column.column [][
            Input.email [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Email"]
        ]
    ]

let inputPw dispatch =
    Container.container [][
        text [][str "Type in your new password and verify your login data."]
        Column.column [][
            Input.password [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Password"]
        ]
    ]

let inputUsernameAdmin dispatch =
    Container.container [][
        text [][str "You are about to change a users account parameters! Type in the new username and verify your login data."]
        Column.column [][
            Input.text [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Username"]
        ]
    ]

let inputEmailAdmin dispatch =
    Container.container [][
        text [][str "You are about to change a users account parameters! Type in the new Email adress and verify your login data."]
        Column.column [][
            Input.email [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Email"]
        ]
    ]

let inputPwAdmin dispatch =
    Container.container [][
        text [][str "You are about to change a users account parameters! Type in the new password and verify your login data."]
        Column.column [][
            Input.password [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Password"]
        ]
    ]

let inputRoleAdmin dispatch =
    Container.container [][
        text [][str "You are about to change a users account parameters! Type in the new role and verify your login data."]
        Column.column [][
            Input.text [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Role"]
        ]
    ]

let userAccountElement model (dispatch : Msg -> unit) (user:User) =
    let elementFallbackIfEmpty =
        if model.User.IsNone || model.Authenticated = false
        then [ str "Access Denied" ]
        else
            let extraElementUserName =
                if model.AdminViewUser.IsNone || user <> model.AdminViewUser.Value
                then VerifyLoginModal (DotnetChangeUserParamRequest (model.LoginModel,Username,""), inputUsername dispatch)
                else VerifyLoginModal (AdminChangeUserParamsRequest (model.LoginModel,model.AdminViewUser.Value,Username,""), inputUsernameAdmin dispatch)
            let extraElementEmail =
                if model.AdminViewUser.IsNone || user <> model.AdminViewUser.Value
                then VerifyLoginModal (DotnetChangeUserParamRequest (model.LoginModel,Email,""), inputEmail dispatch)
                else VerifyLoginModal (AdminChangeUserParamsRequest (model.LoginModel,model.AdminViewUser.Value,Email,""), inputEmailAdmin dispatch)
            let extraElementPassword =
                if model.AdminViewUser.IsNone || user <> model.AdminViewUser.Value
                then VerifyLoginModal (DotnetChangeUserParamRequest (model.LoginModel,Password,""), inputPw dispatch)
                else VerifyLoginModal (AdminChangeUserParamsRequest (model.LoginModel,model.AdminViewUser.Value,Password,""), inputPwAdmin dispatch)
            let extraElementRole =
                VerifyLoginModal (AdminChangeUserParamsRequest (model.LoginModel,model.AdminViewUser.Value,Role,""), inputRoleAdmin dispatch)
            [
                userAccountinformationColumn "Name" user.Username (fun _ -> dispatch (UpdateExtraElement extraElementUserName))
                userAccountinformationColumn "E-Mail"user.Email (fun _ -> dispatch (UpdateExtraElement extraElementEmail))
                Columns.columns [][
                    Column.column [][
                        Heading.h5 [][str "Password"]
                        Heading.h6 [Heading.IsSubtitle] [str "******"]
                    ]
                    Column.column [ Column.Modifiers [Modifier.TextAlignment (Screen.All,TextAlignment.Right)] ][
                        Button.a [ Button.OnClick (fun _ -> dispatch (UpdateExtraElement extraElementPassword)) ][str "Change"]
                    ]
                ]
                Columns.columns [][
                    Column.column [][
                        Heading.h5 [][str "Role"]
                        Heading.h6 [Heading.IsSubtitle][str (string user.Role)]
                    ]
                    Column.column [ Column.Modifiers [Modifier.TextAlignment (Screen.All,TextAlignment.Right)] ][
                        (if AuxFunctions.authentificationLevelByUser model.User >= 5 then Button.a [Button.OnClick (fun _ -> dispatch (UpdateExtraElement extraElementRole))][str "Change"] else str "")
                    ]
                ]
            ]
    let dangerZone =
        let deleteMsg =
            if model.AdminViewUser.IsNone || user <> model.AdminViewUser.Value
            then (fun _ ->
                dispatch (UpdateExtraElement
                    (VerifyLoginModal (DeleteAccountRequest model.LoginModel,str "Delete your account"))
                )
            )
            else ( fun _ ->
                dispatch (UpdateExtraElement
                    (VerifyLoginModal (AdminDeleteAccountRequest (model.LoginModel, model.AdminViewUser.Value), str "You are about to delete a user account. Please verify your login."))
                )
            )
        Columns.columns [
            Columns.Props [
                Style [
                    Border "1.5px solid #ff4d4d"
                    BorderRadius "3px"
                    MarginTop "10px"
        ]]] [
            Column.column [][
                Heading.h6 [][str "Delete this user account"]
                Heading.h6 [Heading.IsSubtitle] [str "Once you delete your account, there is no going back. Please be certain"]
            ]
            Column.column [Column.Width (Screen.All,Column.IsOneFifth);Column.Modifiers [Modifier.TextAlignment (Screen.All,TextAlignment.Right)]][
                Button.button [Button.Color IsDanger;Button.OnClick deleteMsg][
                    str "Delete"
                ]
            ]
        ]
    let backButtonRequest =
        if model.AdminViewUser.IsNone || user <> model.AdminViewUser.Value
        then
            str ""
        else
            Button.button [
                Button.Props [
                    Style [
                        Position PositionOptions.Fixed
                        Bottom "50%"
                        Left "5%"] ]
                Button.OnClick (fun _ -> dispatch (ChangeMainReactElement UserList))
            ][
                Icon.icon [ ][
                    span [ClassName "fas fa-arrow-left"] []
                ]
                span [][str " Back"]
            ]
    div [Style [MarginTop "5%";MarginBottom "5%"]][
        Column.column [ Column.Width (Screen.All,Column.IsHalf);Column.Offset (Screen.All,Column.IsOneQuarter) ][
            Box.box' [Props[Style[Padding "5% 5% 5% 5%"]]]
                (elementFallbackIfEmpty@[dangerZone])
            backButtonRequest
        ]
    ]

/// View for User List Information

let displayUser (user:User) dispatch =
    [|
        tr [][
            td [][str user.Username]
            td [][str user.Email]
            td [][str (string user.Role)]
            span [Style [Padding "auto";MarginLeft "1.5rem"]][
                Button.span [
                    Button.Size IsSmall
                    Button.OnClick (fun _ -> dispatch (AdminSelectUser user))
                ] [
                    str "Edit"
                ]
            ]
        ]
    |]

let dropdownNavbarButtonSize (nameStr:string) dispatchElement =
    Navbar.Item.a
        [ Navbar.Item.Props [ Props.OnClick dispatchElement ];Navbar.Item.CustomClass "dropdownFilter" ]
        [ str nameStr]

let displayAllUsersNavbar model dispatch =
    Navbar.navbar [ Navbar.Props [ Style [
        MarginTop "0.5%";BorderBottom "1px solid lightgrey"; MarginBottom "0.5%";
        JustifyContent "center"; ZIndex "5"
    ]]] [
        Navbar.Item.a [ Navbar.Item.Props [Style [Width "25%"]]][
            Input.search [
                Input.Size Size.IsSmall 
                Input.Placeholder "...search"
                Input.Props [Style [Height "100%"]]
                Input.OnChange (fun e -> dispatch (SortAllUserList e.Value))
            ]
        ]
        Navbar.navbar [Navbar.Props [Style [Width "25%";]]][
            Navbar.Item.a [
                Navbar.Item.Props [Style [MarginLeft "auto";Padding "3px"]]
                Navbar.Item.HasDropdown; Navbar.Item.IsHoverable;
            ] [
                Navbar.Link.a [] [ str (if model.AdminUserListRoleFilter = All then "Role-Filter" else string model.AdminUserListRoleFilter) ]
                Navbar.Dropdown.div [ ] [
                    dropdownNavbarButtonSize "All" (fun _ -> dispatch (FilterAllUserList All))
                    Dropdown.divider []
                    dropdownNavbarButtonSize "User" (fun _ -> dispatch (FilterAllUserList User))
                    dropdownNavbarButtonSize "UserManager" (fun _ -> dispatch (FilterAllUserList UserManager))
                    dropdownNavbarButtonSize "Admin" (fun _ -> dispatch (FilterAllUserList Admin))
                    dropdownNavbarButtonSize "Developer" (fun _ -> dispatch (FilterAllUserList Developer))
                ]
            ]
        ]
        Navbar.Item.a [][
            div [
                OnClick (
                    fun _ ->
                        dispatch ClearRegisterLogin
                        dispatch (UpdateExtraElement AdminRegisterModal)
                )
            ] [
                Fa.span [
                    Fa.Solid.PlusCircle
                    Fa.FixedWidth ] [ ]
                str " Add User"
            ]
        ]
    ]

let displayAllUsersElement (model:Model) dispatch =
    div [Style [MarginBottom "5%"]][
        displayAllUsersNavbar model dispatch
        Column.column [
            Column.Width (Screen.All,Column.IsHalf);Column.Offset (Screen.All,Column.IsOneQuarter)
        ][
            Table.table [
                Table.IsFullWidth
            ] [
                thead [][
                    tr [][
                        th [][str "Username"]
                        th [][str "E-Mail"]
                        th [][str "Role"]
                        span [][]
                    ]
                ]
                tbody []
                    (model.AdminUserList
                    |> Array.filter (fun x -> if model.AdminUserListRoleFilter = All then x = x else x.Role = model.AdminUserListRoleFilter)
                    |> (Array.collect (fun userVal -> displayUser userVal dispatch)
                        >> List.ofArray)
                    )
            ]
        ]
    ]