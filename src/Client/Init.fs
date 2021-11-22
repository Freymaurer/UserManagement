module Init

open Elmish

open Model
open Messages
open Shared

let init () : Model * Cmd<Msg> =
    let model = {
        PageModel       = PageModel.Home <| Model.Todo.Model.init
        UserState       = UserState.init
        NavbarMenuState = false
    }

    let cmd =
        Cmd.OfAsync.perform Api.todosApi.getTodos () (Messages.Todo.GotTodos >> TodoMsg)

    let userCmd = IdentityMsg Identity.GetActiveUserRequest |> Cmd.ofMsg

    model, Cmd.batch [cmd; userCmd]