module Index


open Shared
open Model
open Messages

open Feliz
open Feliz.Bulma

let mainElement (model:Model) dispatch =
    match model.PageModel with
    | PageModel.Home todoModel ->
        Todo.mainElement model todoModel dispatch
    | PageModel.Login loginModel ->
        Login.mainElement model loginModel dispatch
    | PageModel.Signup signupModel ->
        Signup.mainElement model signupModel dispatch

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.hero [
        hero.isFullHeight
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]
        prop.children [
            Bulma.heroHead [
                Navbar.mainNavbarEle model dispatch
            ]
            Bulma.heroBody [
                Bulma.container [
                    Bulma.column [
                        column.is6
                        column.isOffset3
                        prop.children [
                            Bulma.title [
                                text.hasTextCentered
                                color.hasTextWhite
                                prop.text "UserManagement"
                            ]
                            mainElement model dispatch
                        ]
                    ]
                ]
            ]
        ]
    ]
