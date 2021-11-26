module AuthTest

open Model
open Messages.AuthTest
open Messages

open Feliz
open Feliz.Bulma
open Shared
open Elmish
open Fable.React

let init() : AuthTest.Model * Cmd<Msg> =
    let m = AuthTest.Model.init()
    let cmd = Cmd.none
    m, cmd

let update (msg: AuthTest.Msg) (model:Model) (state: AuthTest.Model) : Model * Cmd<Msg> =
    match msg with
    | GetUserHelloRequest ->
        let cmd =
            Cmd.OfAsync.either
                Api.userApi.getHelloUser
                ()
                (GetUserHelloResponse >> AuthTestMsg)
                (curry GenericError Cmd.none)
        model, cmd
    | GetUserHelloResponse responseText ->
        let nextState = { state with Response = Some <| AuthTest.ResponseMsg.User responseText }
        let nextModel = { model with PageModel = PageModel.AuthTest nextState }
        nextModel, Cmd.none
    | GetAdminHelloRequest ->
        let cmd =
            Cmd.OfAsync.either
                Api.adminApi.getHelloAdmin
                ()
                (GetAdminHelloResponse >> AuthTestMsg)
                (curry GenericError Cmd.none)
        model, cmd
    | GetAdminHelloResponse responseText ->
        let nextState = { state with Response = Some <| AuthTest.ResponseMsg.Admin responseText }
        let nextModel = { model with PageModel = PageModel.AuthTest nextState }
        nextModel, Cmd.none

let private getUserHelloButton (model: Model) (state:AuthTest.Model) (dispatch: Msg -> unit) =
    Bulma.button.button [
        color.isInfo
        prop.text "Test User Auth"
        prop.onClick (fun _ -> AuthTestMsg GetUserHelloRequest |> dispatch)
    ]

let private getAdminHelloButton (model: Model) (state:AuthTest.Model) (dispatch: Msg -> unit) =
    Bulma.button.button [
        color.isPrimary
        prop.text "Test Admin Auth"
        prop.onClick (fun _ -> AuthTestMsg GetAdminHelloRequest |> dispatch)
    ]

let mainElement (model: Model) (state:AuthTest.Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.field.div "Click one of the buttons below to test authentication success."
        Bulma.field.div [
            Bulma.buttons [
                Bulma.buttons.isCentered
                prop.children [
                    getUserHelloButton model state dispatch
                    getAdminHelloButton model state dispatch
                ]
            ]
        ]
        match state.Response with
        | None -> Html.div "..."
        | Some (AuthTest.ResponseMsg.User msg) ->
            Html.div [
                color.hasTextInfo
                prop.text msg
            ]
        | Some (AuthTest.ResponseMsg.Admin msg) ->
            Html.div [
                color.hasTextPrimary
                prop.text msg
            ]
    ]