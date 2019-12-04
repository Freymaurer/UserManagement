module Client.View.Modals

open System

open Fable.React
open Fable.React.Props
open Fulma
open Shared
open Fable.FontAwesome

open Client.Types
open Client.View.Helper

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

let addUsernameToExtLoginModal model dispatch =
    Modal.modal [
        Modal.IsActive true
        Modal.Props []
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
                    str "Create your Username"
                ]
            ]
            Modal.Card.body  [ ] [
                text [][
                    str "Thank you for using our external login service!"
                    str "Please enter your preferred username. It will be used to show your activities and will hold correlation to your login method."
                ]
                Box.box' [ Props [ Class "registerBox" ] ] [
                    Text.div [ Props [ Style [ PaddingLeft "1rem" ] ] ] [
                        str "Create your Username"
                    ]
                    Input.text [
                        Input.OnChange (fun e ->
                            let newModel = {model.RegisterModel with Username = e.Value}
                            dispatch (UpdateRegisterModel newModel)
                            )
                        Input.Props [
                            Style [
                                BoxShadow "none"; Border "none";
                                BackgroundColor "#f2f2f2"
                            ]
                        ]
                    ]
                ]
                Columns.columns [ Columns.Props [ Style [ PaddingTop "2rem" ] ] ] [
                    Column.column [Column.Offset (Screen.All,Column.IsHalf)] [
                        Button.button [
                            Button.Color IsInfo
                            (if model.RegisterModel.Username = "" then Button.Disabled true else Button.Disabled false )
                            Button.OnClick (fun _ -> dispatch (AddUsernameToExtLogin model.RegisterModel.Username))
                        ][
                            str "Register Username"
                        ]
                    ]
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

let verifyLoginModal (model : Model) (extraElement: (Model -> (Msg -> unit) -> ReactElement)) (dispatch : Msg -> unit) msgInput =
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
                Modal.Card.title [ Props [ Style [ Padding "0.75em 1em";BorderBottom "1px solid grey" ] ] ] [
                    str "Verify your Login"
                ]
            ]
            Modal.Card.body  [ ] [
                div [][
                    text [][str "Verify your login information. This is done to increase security."]
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
                ]
                extraElement model dispatch
                Columns.columns [ Columns.Props [ Style [ PaddingTop "2rem" ] ] ] [
                    Column.column [Column.Offset (Screen.All,Column.IsHalf)] [
                        Button.button [
                            Button.Color IsDanger
                            (if model.LoginModel.Username = "" || model.LoginModel.Password = "" then Button.Disabled true else Button.Disabled false )
                            Button.OnClick (fun _ -> dispatch msg)
                        ][
                            str "Verify"
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
                            (if model.RegisterModel.Username = "" || model.RegisterModel.Password = "" || model.RegisterModel.Email = "" || model.AdminAssignRole = Guest then Button.Disabled true else Button.Disabled false )
                            Button.OnClick (fun _ -> dispatch (AdminRegisterUserRequest (model.RegisterModel,model.AdminAssignRole)))
                        ][
                            str "Register"
                        ]
                    ]
                ]
            ]
        ]
    ]