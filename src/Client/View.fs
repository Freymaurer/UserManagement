module Client.View

open Fable.React
open Fable.React.Props
open Fulma
open Client.Types
open Client.View

let safeComponents =
    let components =
        span [ ]
           [ a [ Href "https://github.com/SAFE-Stack/SAFE-template" ]
               [ str "SAFE  "
                 str Version.template ]
             str ", "
             a [ Href "https://saturnframework.github.io" ] [ str "Saturn" ]
             str ", "
             a [ Href "http://fable.io" ] [ str "Fable" ]
             str ", "
             a [ Href "https://elmish.github.io" ] [ str "Elmish" ]
             str ", "
             a [ Href "https://fulma.github.io/Fulma" ] [ str "Fulma" ]
             str ", "
             a [ Href "https://zaid-ajaj.github.io/Fable.Remoting/" ] [ str "Fable.Remoting" ]
           ]

    span [ ]
        [ str "Version "
          strong [ ] [ str Version.app ]
          str " powered by: "
          components ]

let extraEle model dispatch =
    match model.ExtraReactElement with
    | EmptyElement -> Helper.emptyStr
    | RegisterModal -> Modals.registerModal model dispatch
    | AdminRegisterModal -> Modals.adminRegisterModal model dispatch
    | Message x -> Helper.messageContainer x (fun _ -> dispatch (UpdateExtraElement EmptyElement))
    | VerifyLoginModal (x,ele) -> Modals.verifyLoginModal model ele dispatch x
    | AddUsernameToExternLoginModal -> Modals.addUsernameToExtLoginModal model dispatch

let view (model : Model) (dispatch : Msg -> unit) =
    div [ ] [
        // side menu coming in from the left
        SidebarMenu.menuCard model dispatch
        // top navbar
        Navbar.navbar [ Navbar.Color IsWhiteBis; Navbar.Props [Style [BorderBottom "1px solid grey"]] ]
            (if model.Authenticated = true then (NavbarLogin.loggedInNavbar model dispatch) else (NavbarLogin.loginNavbar model dispatch ))
        // extra elements, such as messages or modals
        extraEle model dispatch
        // Main Element rendering
        (
            match model.MainReactElement with
            | Welcome -> Placeholder.welcomeElement model dispatch
            | Counter -> Counter.counter model dispatch
            | UserAccount user -> UserAccount.userAccountElement model dispatch user
            | UserList -> Userlist.displayAllUsersElement model dispatch
            //| _ -> Placeholder.constructionLabel model dispatch
        )
        //Button.button [Button.OnClick (fun _ -> dispatch GetContextClaimsRequest)][str "Get Claims"]
        Footer.footer [ ]
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                  [ safeComponents ] ] ]