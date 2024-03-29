module Navbar

open Model
open Feliz
open Feliz.Bulma
open Messages
open Fable.FontAwesome
open Fable.React
open Feliz

let private bulmaLogo =
    Bulma.navbarItem.a [
        Html.img [ prop.src "https://bulma.io/images/bulma-logo-white.png"; prop.height 28; prop.width 112; ]
    ]

let private navigationElements dispatch =
    Bulma.navbarStart.div [
        Bulma.navbarItem.a [
            prop.text "Home"
            prop.onClick (fun _ ->
                UpdatePage Route.Todo |> dispatch
            )
        ]
        Bulma.navbarItem.a [
            prop.text "Auth_Test"
            prop.onClick (fun _ ->
                UpdatePage Route.AuthTest |> dispatch
            )
        ]
    ]

let private loginButton dispatch =
    Bulma.button.a [
        prop.text "Log In"
        prop.onClick (fun _ -> UpdatePage Route.Login |> dispatch )
    ]

let private signupButton dispatch =
    Bulma.button.a [
        prop.text "Sign Up"
        prop.onClick (fun _ -> UpdatePage Route.Signup |> dispatch )
    ]

let private settingsItem dispatch =
    Bulma.navbarItem.a [
        prop.onClick (fun _ -> UpdatePage Route.Profile |> dispatch)
        prop.children [
            Html.i [prop.className "fas fa-user-cog fa-fw"; prop.style [style.marginRight (length.rem 1)]]
            Html.span "Settings"
        ]
    ]

let private adminUsersItem dispatch =
    Bulma.navbarItem.a [
        prop.onClick (fun _ -> UpdatePage Route.AdminUsers |> dispatch)
        prop.children [
            Html.i [prop.className "fas fa-users fa-fw"; prop.style [style.marginRight (length.rem 1)]]
            Html.span "Users"
        ]
    ]


let private logoutItem dispatch =
    Bulma.navbarItem.a [
        prop.onClick (fun _ -> IdentityMsg Identity.LogoutRequest |> dispatch)
        prop.children [
            Html.i [prop.className "fas fa-sign-out-alt fa-fw"; prop.style [style.marginRight (length.rem 1)]]
            Html.span "Log Out"
        ]
    ]

let private loginElements (model:Model) dispatch =
    Bulma.navbarEnd.div [
        Bulma.navbarItem.div [
            Bulma.buttons [
                Bulma.buttons.areSmall
                prop.children [
                    loginButton dispatch
                    signupButton dispatch
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
                    Bulma.navbarLink.isArrowless
                    prop.children [
                        Html.i [prop.className "fas fa-user"; prop.style [style.marginRight (length.rem 1)]]
                        Html.span model.UserState.User.Value.Username
                    ]
                ]
                Bulma.navbarDropdown.div [
                    Bulma.navbarDropdown.isBoxed
                    Bulma.navbarDropdown.isRight
                    prop.children [
                        settingsItem dispatch
                        if model.UserState.User.Value.Role.isAdmin then adminUsersItem dispatch
                        Bulma.navbarDivider []
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