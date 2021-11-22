module Navbar

open Model
open Feliz
open Feliz.Bulma
open Messages
open Fable.FontAwesome
open Fable.React

let private bulmaLogo =
    Bulma.navbarItem.a [
        Html.img [ prop.src "https://bulma.io/images/bulma-logo-white.png"; prop.height 28; prop.width 112; ]
    ]

let private navigationElements dispatch =
    Bulma.navbarStart.div [
        Bulma.navbarItem.a [
            prop.text "Home"
            prop.onClick (fun _ -> UpdatePageModel (PageModel.Home Todo.Model.init) |> dispatch)
        ]
    ]

let private loginButton (model:Model) dispatch =
    Bulma.button.a [
        prop.text "Log In"
        prop.onClick (fun _ -> UpdatePageModel (PageModel.Login Login.Model.init) |> dispatch )
    ]

let private profileItem dispatch =
    Bulma.navbarItem.a [
        Html.i [prop.className "fa fa-id-badge fa-fw fa-pull-left"]
        Html.span "Profile"
    ]


let private logoutItem dispatch =
    Bulma.navbarItem.a [
        prop.onClick (fun _ -> IdentityMsg Identity.LogoutRequest |> dispatch)
        prop.children [
            Html.i [prop.className "fa fa-sign-out-alt fa-fw fa-pull-left"]
            Html.span "Log Out"
        ]
    ]

let private loginElements (model:Model) dispatch =
    Bulma.navbarEnd.div [
        Bulma.navbarItem.div [
            Bulma.buttons [
                Bulma.buttons.areSmall
                prop.children [
                    loginButton model dispatch
                    Bulma.button.a [ prop.text "Sign up" ]
                ]
            ]
        ]
    ]

let private loggedInElement (model:Model) dispatch =
    Bulma.navbarEnd.div [
        Bulma.navbarItem.div [
            Bulma.navbarItem.hasDropdown
            Bulma.navbarItem.isHoverable
            Bulma.color.isLight
            prop.children [
                Bulma.navbarLink.a [
                    Fa.i [Fa.Solid.User; Fa.PullLeft] []
                    span [][str model.UserState.User.Value.Username]
                ]
                Bulma.navbarDropdown.div [
                    Bulma.navbarDropdown.isBoxed
                    prop.children [
                        profileItem dispatch
                        logoutItem dispatch
                    ]
                ]
            ]
        ]
    ]

let mainNavbarEle (model:Model) dispatch =
    Bulma.navbar [
        prop.children [
            Bulma.navbarBrand.div [
                bulmaLogo
                Bulma.navbarBurger [
                    if model.NavbarMenuState then navbarBurger.isActive
                    prop.onClick (fun _ ->
                        let nextState = model.NavbarMenuState |> not
                        UpdateNavbarMenuState nextState |> dispatch
                    )
                    prop.children [
                        Html.span [prop.ariaHidden true]
                        Html.span [prop.ariaHidden true]
                        Html.span [prop.ariaHidden true]
                    ]
                ]
            ]
            Bulma.navbarMenu [
                if model.NavbarMenuState then Bulma.navbarMenu.isActive
                prop.children [
                    navigationElements dispatch
                    if model.UserState.LoggedIn && model.UserState.User.IsSome then
                        loggedInElement model dispatch
                    else
                        loginElements model dispatch
                ]
            ]
        ]
    ]