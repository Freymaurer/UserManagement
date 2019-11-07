module Client.App

open System

open Elmish
open Elmish.React
open Fable.React
open Fulma

open Client.Types
open Client.State
open Client.View
open Fable
open Fable.React
open Fable.React.Props


let extraEle model dispatch =
    match model.ExtraReactElement with
    | EmptyElement -> emptyStr
    | RegisterModal -> registerModal model dispatch
    | AdminRegisterModal -> adminRegisterModal model dispatch
    | Message x -> messageContainer x (fun _ -> dispatch (UpdateExtraElement EmptyElement))
    | VerifyLoginModal (x,ele) -> verifyLoginModal model ele dispatch x

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
        menuCard model dispatch
        Navbar.navbar [ Navbar.Color IsPrimary ]
            (if model.Authenticated = true then (loggedInNavbar model dispatch) else (loginNavbar model dispatch ))
        extraEle model dispatch
        // Menu rendering
        (
            match model.MainReactElement with
            | Counter -> counter model dispatch
            | UserAccount user -> userAccountElement model dispatch user
            | UserList -> displayAllUsersElement model dispatch
            | _ -> constructionLabel model dispatch
        )
        Footer.footer [ ]
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                  [ safeComponents ] ] ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run