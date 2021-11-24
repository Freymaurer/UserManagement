module Settings

open Model
open Messages.Settings
open Messages

open Feliz
open Feliz.Bulma
open Shared
open Elmish

let init() : Settings.Model * Cmd<Msg> =
    let m = Settings.Model.init
    let cmd = Cmd.none
    m, cmd

let update (msg: Settings.Msg) (model:Model) (state: Settings.Model) : Model * Cmd<Msg> =
    match msg with
    | DefaultMsg ->
        model, Cmd.none

let mainElement (model: Model) (todoModel:Settings.Model) (dispatch: Msg -> unit) =
    Bulma.box [
        prop.text "Hello!"
    ]