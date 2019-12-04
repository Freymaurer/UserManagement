module Client.View.UserAccount

open Fable.React
open Fable.React.Props
open Fulma
open Shared
open Fable.FontAwesome

open Client.Types

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

let guestAccountInformationColumn headerStr informationStr =
    Columns.columns [][
        Column.column [][
            Heading.h5 [][str headerStr]
            Heading.h6 [Heading.IsSubtitle][str informationStr]
        ]
    ]

    ///needs empty model because it will be given to modal functions that always wants a (model -> (Msg -> unit) -> ReactElement) function
let inputUsername model dispatch =
    Container.container [][
        text [][str "Type in your new username."]
        Column.column [][
            Input.text [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Username"]
        ]
    ]

let inputEmail model dispatch =
    Container.container [][
        text [][str "Type in your new Email adress."]
        Column.column [][
            Input.email [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Email"]
        ]
    ]

let inputPw model dispatch =
    Container.container [][
        text [][str "Type in your new password."]
        Column.column [][
            Input.password [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Password"]
        ]
    ]

let inputUsernameAdmin model dispatch =
    Container.container [][
        text [][str "You are about to change a users account parameters! Type in the new username."]
        Column.column [][
            Input.text [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Username"]
        ]
    ]

let inputEmailAdmin model dispatch =
    Container.container [][
        text [][str "You are about to change a users account parameters! Type in the new Email adress."]
        Column.column [][
            Input.email [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Email"]
        ]
    ]

let inputPwAdmin model dispatch =
    Container.container [][
        text [][str "You are about to change a users account parameters! Type in the new password."]
        Column.column [][
            Input.password [Input.OnChange (fun e -> dispatch (UpdateInputString e.Value)); Input.Placeholder "... New Password"]
        ]
    ]

let inputRoleAdmin (model:Model) dispatch =
    Container.container [][
        text [][str "You are about to change a users account parameters! Type in the new role."]
        Column.column [][
            Dropdown.dropdown [ Dropdown.IsHoverable;Dropdown.IsUp] [
                div [ ] [
                    Button.button [ Button.Modifiers [Modifier.BackgroundColor IsWhiteTer] ] [
                        span [ ] [ str (if model.InputString = "" then "Role" else model.InputString)]
                        Icon.icon [ Icon.Size IsSmall ] [ Fa.i [ Fa.Solid.AngleDown ] [ ] ]
                    ]
                ]
                Dropdown.menu [ ] [
                    Dropdown.content [ ] [
                        Dropdown.Item.a [ Dropdown.Item.Props [OnClick (fun _ -> dispatch (UpdateInputString (string ActiveUserRoles.Admin))) ] ] [ str "Admin" ]
                        Dropdown.Item.a [ Dropdown.Item.Props [OnClick (fun _ -> dispatch (UpdateInputString (string ActiveUserRoles.UserManager))) ] ] [ str "UserManager" ]
                        Dropdown.divider [ ]
                        Dropdown.Item.a [ Dropdown.Item.Props [OnClick (fun _ -> dispatch (UpdateInputString (string ActiveUserRoles.User))) ] ] [ str "User" ]
                    ]
                ]
            ]
        ]
    ]

let userAccountElement model (dispatch : Msg -> unit) (user:User) =
    ///needed because it will be given to modal functions that always wants a (model -> (Msg -> unit) -> ReactElement) function
    let strReactElement stringVal model dispatch =
        str stringVal
    let isNotGuest (userOpt: Shared.User) =
        if userOpt.Role <> Shared.Guest then true else false
    let userAccountEle =
        if model.User.IsNone || model.Authenticated = false
        then [ str "Access Denied" ]
        else
            //check for admin or user and give back different modals when changing account parameters
            let extraElementUserName =
                if model.AdminViewUser.IsNone || user <> model.AdminViewUser.Value
                then VerifyLoginModal (DotnetChangeUserParamRequest (model.LoginModel,Username,""), inputUsername)
                else VerifyLoginModal (AdminChangeUserParamsRequest (model.LoginModel,model.AdminViewUser.Value,Username,""), inputUsernameAdmin)
            let extraElementEmail =
                if model.AdminViewUser.IsNone || user <> model.AdminViewUser.Value
                then VerifyLoginModal (DotnetChangeUserParamRequest (model.LoginModel,Email,""), inputEmail)
                else VerifyLoginModal (AdminChangeUserParamsRequest (model.LoginModel,model.AdminViewUser.Value,Email,""), inputEmailAdmin)
            let extraElementPassword =
                if model.AdminViewUser.IsNone || user <> model.AdminViewUser.Value
                then VerifyLoginModal (DotnetChangeUserParamRequest (model.LoginModel,Password,""), inputPw)
                else VerifyLoginModal (AdminChangeUserParamsRequest (model.LoginModel,model.AdminViewUser.Value,Password,""), inputPwAdmin)
            let extraElementRole =
                VerifyLoginModal (AdminChangeUserParamsRequest (model.LoginModel,model.AdminViewUser.Value,Role,""), inputRoleAdmin)
            [
                (if isNotGuest user then userAccountinformationColumn "Name" user.Username (fun _ -> dispatch (UpdateExtraElement extraElementUserName)) else guestAccountInformationColumn "Name" user.Username)
                (if isNotGuest user then userAccountinformationColumn "E-Mail" user.Email (fun _ -> dispatch (UpdateExtraElement extraElementEmail)) else guestAccountInformationColumn "E-Mail" user.Email)
                Columns.columns [][
                    Column.column [][
                        Heading.h5 [][str "Password"]
                        Heading.h6 [Heading.IsSubtitle] [str "******"]
                    ]
                    Column.column [ Column.Modifiers [Modifier.TextAlignment (Screen.All,TextAlignment.Right)] ][
                        (if isNotGuest user then Button.a [ Button.OnClick (fun _ -> dispatch (UpdateExtraElement extraElementPassword)) ][str "Change"] else str "")
                    ]
                ]
                Columns.columns [][
                    Column.column [][
                        Heading.h5 [][str "Role"]
                        Heading.h6 [Heading.IsSubtitle][str (string user.Role)]
                    ]
                    Column.column [ Column.Modifiers [Modifier.TextAlignment (Screen.All,TextAlignment.Right)] ][
                        (if AuxFunctions.authentificationLevelByUser model.User >= 5 && isNotGuest user then Button.a [Button.OnClick (fun _ -> dispatch (UpdateExtraElement extraElementRole))][str "Change"] else str "")
                    ]
                ]
                Columns.columns [][
                    Column.column [][
                        Heading.h5 [][str "Account Origin"]
                        Heading.h6 [Heading.IsSubtitle][str user.AccountOrigin]
                    ]
                ]
                Columns.columns [][
                    Column.column [][
                        Heading.h5 [][str "Unique Identifier"]
                        Heading.h6 [Heading.IsSubtitle][str user.UniqueId]
                    ]
                ]
            ]
    let dangerZone =
        let deleteMsg =
            if model.AdminViewUser.IsNone || user <> model.AdminViewUser.Value
            then (fun _ ->
                dispatch (UpdateExtraElement
                    (VerifyLoginModal (DeleteAccountRequest model.LoginModel,strReactElement "Delete your account"))
                )
            )
            else ( fun _ ->
                dispatch (UpdateExtraElement
                    (VerifyLoginModal (AdminDeleteAccountRequest (model.LoginModel, model.AdminViewUser.Value), strReactElement "You are about to delete a user account. Please verify your login."))
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
                Heading.h6 [Heading.IsSubtitle] [ (if user = model.User.Value then str "Once you delete your account, there is no going back. Please be certain" else str "Once you delete this account, there is no going back. Please be certain")]
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
                (if isNotGuest user then (userAccountEle@[dangerZone]) else (userAccountEle@[str ""]))
            backButtonRequest
        ]
    ]
