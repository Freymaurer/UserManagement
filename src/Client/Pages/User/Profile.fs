module Profile

open Model
open Messages.Profile
open Messages

open Feliz
open Feliz.Bulma
open Shared
open Elmish
open Fable.React

let init(loginInfo: IdentityTypes.User option) : Profile.Model * Cmd<Msg> =
    let m = if loginInfo.IsSome then Profile.Model.init(loginInfo.Value) else Profile.Model.init()
    let cmd = Cmd.none
    m, cmd

let update (msg: Profile.Msg) (model:Model) (state: Profile.Model) : Model * Cmd<Msg> =
    match msg with
    | UpdateNewProfileInfo newInfo ->
        let nextState = { state with NewProfileInfo = Some newInfo }
        let nextModel = { model with PageModel = PageModel.Profile nextState }
        nextModel, Cmd.none

let private isChangedIcon =
    Bulma.icon [
        Bulma.icon.isSmall
        Bulma.icon.isRight
        prop.children [
            Html.i [
                prop.className "fas fa-user-edit"
            ]
        ]
    ]

let private userNameControl (model:Model) (state:Profile.Model) dispatch =
    let currentData = model.UserState.User.Value.Username
    let isChanged = currentData <> state.NewProfileInfo.Value.Username
    Bulma.field.div [
        Bulma.label "Username"
        Bulma.control.div [
            Bulma.control.hasIconsRight
            prop.children [
                Bulma.input.text [
                    if isChanged then color.isSuccess
                    prop.defaultValue state.NewProfileInfo.Value.Username
                    prop.onChange (fun (e:Browser.Types.Event) ->
                        let newInfo = {state.NewProfileInfo.Value with Username = e.Value}
                        UpdateNewProfileInfo newInfo |> ProfileMsg |> dispatch
                    )
                ]
                if isChanged then isChangedIcon
            ]
        ]
    ]

let private emailControl (model:Model) (state:Profile.Model) dispatch =
    let currentData = model.UserState.User.Value.Email
    let isChanged = currentData <> state.NewProfileInfo.Value.Email
    Bulma.field.div [
        Bulma.label "Email"
        Bulma.control.div [
            Bulma.control.hasIconsRight
            prop.children [
                Bulma.input.text [
                    if isChanged then color.isSuccess
                    prop.defaultValue model.UserState.User.Value.Email
                    prop.onChange (fun (e:Browser.Types.Event) ->
                        let newInfo = {state.NewProfileInfo.Value with Email = e.Value}
                        UpdateNewProfileInfo newInfo |> ProfileMsg |> dispatch
                    )
                ]
                if isChanged then isChangedIcon
            ]
        ]
    ]

let private accountOriginElement (model:Model) dispatch =
    Bulma.field.div [
        Bulma.label "Account Origin"
        Bulma.control.div [
            Bulma.input.text [
                prop.readOnly true
                prop.defaultValue model.UserState.User.Value.AccountOrigin
            ]
        ]
    ]

let private userRoleElement (model:Model) dispatch =
    Bulma.field.div [
        Bulma.label "User Role"
        Bulma.control.div [
            Bulma.input.text [
                prop.readOnly true
                prop.defaultValue (string model.UserState.User.Value.Role)
            ]
        ]
    ]

let updateButton (model:Model) (state:Profile.Model) dispatch =
    Bulma.field.div [
        Bulma.field.isGrouped
        Bulma.field.isGroupedCentered
        prop.children [
            Bulma.control.div [
                Bulma.button.button [
                    color.isSuccess
                    prop.text "Update Profile"
                    prop.onClick (fun _ ->
                        let isDifferent = model.UserState.User <> state.NewProfileInfo && model.UserState.User.IsSome && state.NewProfileInfo.IsSome
                        if isDifferent then
                            Identity.UpdateUserProfileRequest state.NewProfileInfo.Value |> IdentityMsg |> dispatch
                    )
                ]
            ]
        ]
    ]

let mainElement (model: Model) (state:Profile.Model) (dispatch: Msg -> unit) =
    Bulma.box [

        Bulma.title "Profile"

        userNameControl model state dispatch
        emailControl model state dispatch
        accountOriginElement model dispatch
        userRoleElement model dispatch

        updateButton model state dispatch
    ]