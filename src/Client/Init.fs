module Init

open Elmish

open Model
open Messages
open Shared

let init () : Model * Cmd<Msg> =
    let state, cmd = Todo.init()
    let model = {
        PageModel       = PageModel.Todo state
        UserState       = UserState.init
        NavbarMenuState = false
    }
    let userCmd =
        Cmd.OfAsync.perform
            Api.userApi.getActiveUser
            ()
            (Identity.GetActiveUserResponse >> IdentityMsg)
    model, Cmd.batch [cmd; userCmd]