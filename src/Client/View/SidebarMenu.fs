module Client.View.SidebarMenu

open Fable.React
open Fable.React.Props
open Fulma
open Shared
open Fable.FontAwesome

open Client.Types

// Helper to generate a menu item
let menuItem label msg =
    Menu.Item.li [ Menu.Item.OnClick msg]
        [ str label ]

// Helper to generate a sub menu
let subMenu label children =
    li [ ] [
        Menu.Item.a [ ]
           [ str label ]
        ul [ ] children
    ]

let menu (model:Model) dispatch =
    let hideElementsBy threshold=
        Screen.All, if AuxFunctions.authentificationLevelByUser model.User >= threshold then false else true
    let unAuthenticated =
        div [
            Style [BackgroundColor "white";PaddingTop "1rem";PaddingLeft "1rem";Height "100%";]
        ][
            str "This site is meant for account management. Please log in to use the provided functions."
        ]
    let authenticated =
        Menu.menu [
            Modifiers [ Modifier.TextSize (Screen.All,TextSize.Is7)]
            Props [Style [BackgroundColor "white";PaddingTop "1rem";PaddingLeft "1rem";Height "100%"]]
        ] [
            Menu.list [ ]
                [  ]
            Menu.label [ ] [ str "User Account" ]
            Menu.list [ ] [
                menuItem "Home" (fun _ -> dispatch (ChangeMainReactElement Welcome))
                menuItem "Account Information" (fun _ -> dispatch (ChangeMainReactElement (UserAccount model.User.Value))) ]
            Menu.label [ Modifiers [Modifier.IsHidden (hideElementsBy 5)] ] [ str "Account Management" ]
            Menu.list [ Modifiers [Modifier.IsHidden (hideElementsBy 5)] ] [
                menuItem "User List"  (fun _ ->
                dispatch AdminGetAllUsersRequest
                dispatch (ChangeMainReactElement UserList))
            ]
            Menu.label [ Modifiers [Modifier.IsHidden (hideElementsBy 10)] ] [str "Debug"]
            Menu.list [Modifiers [Modifier.IsHidden (hideElementsBy 10)]]
                [menuItem "Test Counter" (fun _ -> dispatch (ChangeMainReactElement Counter))]
        ]
    match model.Authenticated with
    | true -> authenticated
    | false -> unAuthenticated

let menuCard model dispatch =
    div [
        Style [
            Position PositionOptions.Absolute;
            BackgroundColor "rgba(0, 0, 0, 0.5)"
            ZIndex "100"
            Width "100%"
            Height "100%"
            Transition "Visibility"
            TransitionDuration (if model.ShowMenuBool = true then "0s" else ".50s")
            Visibility (if model.ShowMenuBool = true then "visible" else "hidden")
        ]
    ][
        /// just a background element that will toggle off the menu when clicked on
        div [OnClick (fun _ -> dispatch ToggleMenu);Style [Width "100%";Height "100%";Position PositionOptions.Absolute;ZIndex "200"]][]
        /// The menu itself
        Column.column [
            Column.Width (Screen.All,Column.Is2);
            Column.Modifiers [ Modifier.IsPaddingless ];
            Column.Props [
                Style [
                    Height "100%";ZIndex "300"; Position PositionOptions.Absolute
                    Transform (if model.ShowMenuBool = true then "translate3d(0, 0, 0)" else "translate3d(-100%, 0, 0)")
                    TransitionDuration ".50s"
                    TransitionProperty "transform"
                ]
            ]
        ] [
            Navbar.navbar [ Navbar.Color IsBlack ] [
                Navbar.Item.div [Navbar.Item.Props [ Style [ MarginLeft "1rem"; MarginRight "0.5rem" ;MinHeight "3.25rem"] ]][
                    Fa.i [
                        Fa.Solid.Bars
                        Fa.Props [
                            OnClick (fun _ ->  dispatch ToggleMenu)
                            Style [
                                Cursor "pointer"
                            ]
                        ]
                    ] [ ]
                ]
                Navbar.Item.div [Navbar.Item.Props [ Style [ MarginLeft "1rem"; MarginRight "0.5rem" ;MinHeight "3.25rem"] ]][
                    div [ Style[Color "#e6e6e6"] ][str (if model.User.IsSome then model.User.Value.Username else "Log In")]
                ]
            ]
            menu model dispatch
        ]
    ]
