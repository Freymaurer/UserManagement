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
open System.IO

let path = Environment.CurrentDirectory

let extraEle model dispatch =
    match model.ExtraReactElement with
    | EmptyElement -> emptyStr
    | RegisterModal -> registerModal model dispatch
    | AdminRegisterModal -> adminRegisterModal model dispatch
    | Message x -> messageContainer x (fun _ -> dispatch (UpdateExtraElement EmptyElement))
    | VerifyLoginModal (x,ele) -> verifyLoginModal model ele dispatch x
    | AddUsernameToExternLoginModal -> addUsernameToExtLoginModal model dispatch

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
        menuCard model dispatch
        Navbar.navbar [ Navbar.Color IsWhiteBis; Navbar.Props [Style [BorderBottom "1px solid grey"]] ]
            (if model.Authenticated = true then (loggedInNavbar model dispatch) else (loginNavbar model dispatch ))
        extraEle model dispatch
        // Main Element rendering
        (
            match model.MainReactElement with
            | Welcome -> welcomeElement model dispatch
            | Counter -> counter model dispatch
            | UserAccount user -> userAccountElement model dispatch user
            | UserList -> displayAllUsersElement model dispatch
            | _ -> constructionLabel model dispatch
        )
        //Button.button [Button.OnClick (fun _ -> dispatch GetContextClaimsRequest)][str "Get Claims"]
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