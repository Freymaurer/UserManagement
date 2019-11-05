module Client.App

open System

open Elmish
open Elmish.React
open Fable.React
open Fulma

open Client.Types
open Client.State
open Client.View

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
        menuCard model dispatch
        Navbar.navbar [ Navbar.Color IsPrimary ]
            (if model.Authenticated = true then (loggedInNavbar model dispatch) else (loginNavbar model dispatch ))
        // Menu rendering
        (
            match model.MainReactElement with
            | Counter -> counter model dispatch
            | UserAccount -> userAccountElement model dispatch
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