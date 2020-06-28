module Client.View.Userlist

open Fable.React
open Fable.React.Props
open Fulma
open Shared
open Fable.FontAwesome

open Client.Types

/// View for User List Information

let displayUser (user:User) dispatch =
    [|
        tr [][
            td [][str user.Username]
            td [][str user.Email]
            td [][str (string user.Role)]
            span [Style [Padding "auto";MarginLeft "1.5rem"]][
                Button.span [
                    Button.Size IsSmall
                    Button.OnClick (fun _ -> dispatch (AdminSelectUser user))
                ] [
                    str "Edit"
                ]
            ]
        ]
    |]

let dropdownNavbarButtonSize (nameStr:string) dispatchElement =
    Navbar.Item.a
        [ Navbar.Item.Props [ Props.OnClick dispatchElement ];Navbar.Item.CustomClass "dropdownFilter" ]
        [ str nameStr]

let displayAllUsersNavbar model dispatch =
    Navbar.navbar [ Navbar.Props [ Style [
        PaddingTop "0.5%";BorderBottom "1px solid lightgrey"; MarginBottom "0.5%";
        JustifyContent "center"; ZIndex "5"
    ]]] [
        Navbar.Item.a [ Navbar.Item.Props [Style [Width "25%"]]][
            Input.search [
                Input.Size Size.IsSmall
                Input.Placeholder "...search"
                Input.Props [Style [Height "100%"]]
                Input.OnChange (fun e -> dispatch (SortAllUserList e.Value))
            ]
        ]
        Navbar.navbar [Navbar.Props [Style [Width "25%";]]][
            Navbar.Item.a [
                Navbar.Item.Props [Style [MarginLeft "auto";Padding "3px"]]
                Navbar.Item.HasDropdown; Navbar.Item.IsHoverable;
            ] [
                Navbar.Link.a [] [ str (if model.AdminUserListRoleFilter = None then "Role-Filter" else string model.AdminUserListRoleFilter) ]
                Navbar.Dropdown.div [ ] [
                    dropdownNavbarButtonSize "All" (fun _ -> dispatch (FilterAllUserList None))
                    Dropdown.divider []
                    dropdownNavbarButtonSize "User" (fun _ -> dispatch (FilterAllUserList (Some User)))
                    dropdownNavbarButtonSize "UserManager" (fun _ -> dispatch (FilterAllUserList (Some UserManager)))
                    dropdownNavbarButtonSize "Admin" (fun _ -> dispatch (FilterAllUserList (Some Admin)))
                    dropdownNavbarButtonSize "Developer" (fun _ -> dispatch (FilterAllUserList (Some Developer)))
                ]
            ]
        ]
        Navbar.Item.a [][
            div [
                OnClick (
                    fun _ ->
                        dispatch ClearRegisterLogin
                        dispatch (UpdateExtraElement AdminRegisterModal)
                )
            ] [
                Fa.span [
                    Fa.Solid.PlusCircle
                    Fa.FixedWidth ] [ ]
                str " Add User"
            ]
        ]
    ]

let displayAllUsersElement (model:Model) dispatch =
    div [Style [MarginBottom "5%"]][
        displayAllUsersNavbar model dispatch
        Column.column [
            Column.Width (Screen.All,Column.IsHalf);Column.Offset (Screen.All,Column.IsOneQuarter)
        ][
            Table.table [
                Table.IsFullWidth
            ] [
                thead [][
                    tr [][
                        th [][str "Username"]
                        th [][str "E-Mail"]
                        th [][str "Role"]
                        span [][]
                    ]
                ]
                tbody []
                    (model.AdminUserList
                    |> Array.filter (fun x -> if model.AdminUserListRoleFilter = None then x = x else x.Role = model.AdminUserListRoleFilter.Value)
                    |> (Array.collect (fun userVal -> displayUser userVal dispatch)
                        >> List.ofArray)
                    )
            ]
        ]
    ]