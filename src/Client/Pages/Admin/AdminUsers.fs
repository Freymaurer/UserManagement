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
    let cmd = Cmd.none
    m, cmd

let update (msg: AdminUsers.Msg) (model:Model) (state: AdminUsers.Model) : Model * Cmd<Msg> =
    match msg with
    | DefaultMsg ->
        model, Cmd.none

let mainElement (model: Model) (state:AdminUsers.Model) (dispatch: Msg -> unit) =
    Bulma.box [
        prop.text "Hi!"
    ]