module AdminUsers

open Model
open Messages.AdminUsers
open Messages

open Feliz
open Feliz.Bulma
open Shared
open Elmish
open Fable.React

let init() : AdminUsers.Model * Cmd<Msg> =
    let m = AdminUsers.Model.init()
    let cmd =
        Cmd.OfAsync.either
            Api.adminApi.getUsers
            ()
            (GetUsersResponse >> AdminUsersMsg)
            (curry GenericError Cmd.none)
    m, cmd

let update (msg: AdminUsers.Msg) (model:Model) (state: AdminUsers.Model) : Model * Cmd<Msg> =
    match msg with
    | GetUsersRequest ->
        let cmd =
            Cmd.OfAsync.either
                Api.adminApi.getUsers
                ()
                (GetUsersResponse >> AdminUsersMsg)
                (curry GenericError Cmd.none)
        model, cmd
    | GetUsersResponse users ->
        let nextState = {state with Users = users}
        let nextModel = {model with PageModel = PageModel.AdminUsers nextState}
        nextModel, Cmd.none

let private noUsersElement =
    Bulma.notification [
        prop.text "Unable to retrive any users."
    ]

let userDisplayElement (model: Model) (state:AdminUsers.Model) (dispatch: Msg -> unit) =
    Bulma.tableContainer [
        Bulma.table [
            Bulma.table.isStriped
            Bulma.table.isHoverable
            Bulma.table.isFullWidth
            prop.children [
                Html.thead [
                    Html.tr [
                        Html.th "UserName"
                        Html.th "Email"
                        Html.th "Role"
                    ]
                ]
                Html.tbody [
                    for user in state.Users do
                        yield
                            Html.tr [
                                Html.td user.Username
                                Html.td user.Email
                                Html.td (string user.Role)
                            ]
                ]
            ]
        ]
    ]

let mainElement (model: Model) (state:AdminUsers.Model) (dispatch: Msg -> unit) =
    Bulma.box [
        if Array.isEmpty state.Users then
            noUsersElement
        else
            userDisplayElement model state dispatch
    ]