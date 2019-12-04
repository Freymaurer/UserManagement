module Client.View.NavbarLogin

open System

open Fable.React
open Fable.React.Props
open Fulma
open Shared
open Fable.FontAwesome

open Client.Types
open Client.View

/// https://codepen.io/davidelrizzo/pen/vEYvyv
let googleOAuthButton =
    div [ Props.Class "loginBtn loginBtn--google" ] [
        str "Sign in with Google"
        a [Href OAuthSigninPaths.googleOAuth][
            span [ Class "divToLinkEmptySpan" ] []
        ]
    ]

let githubOAuthButton =
    div [ Props.Class "loginBtn loginBtn--github"][
        str "Sign in with GitHub"
        a [ Href OAuthSigninPaths.githubOAuth ][
            span [ Class "divToLinkEmptySpan" ] []
        ]
    ]

let orcidOAuthButton =
    div [ Props.Class "loginBtn loginBtn--orcid"][
        str "Sign in with Orcid"
        a [ Href OAuthSigninPaths.orcidOAuth ][
            span [ Class "divToLinkEmptySpan" ] []
        ]
    ]

let loginNavbar (model : Model) (dispatch : Msg -> unit) = [
    Navbar.Item.div [ Navbar.Item.Props [ Style [ MarginLeft "1rem"; MarginRight "0.5rem" ] ] ] [
        Fa.i [
            Fa.Solid.Bars
            Fa.Props [
                OnClick (fun _ ->  dispatch ToggleMenu)
                Style [
                    Cursor "pointer"
                ]
            ]
        ] []
    ]
    Navbar.Item.div [ ] [
        Heading.h2 [ ] [ str "SAFE Template - Login" ]
    ]
    Navbar.End.a [ ] [
        Navbar.Item.a [
            Navbar.Item.Props [
                Style [ MarginRight "2rem" ];
                Class "myDropdown"
            ]
        ] [
            Navbar.Item.a [ ] [
                Text.div
                    [
                        Modifiers [ Modifier.TextWeight TextWeight.SemiBold; Modifier.TextColor Color.IsBlackBis ];
                        Props [Style [MarginTop "20%"; MarginBottom "20%"]]
                    ]
                    [ str "Log In"]
            ]
            Navbar.Dropdown.div [
                Navbar.Dropdown.Props [ Style [ Width "15rem" ]; Class "myDropdown-content" ]
            ] [
                Navbar.Item.div
                    [ Navbar.Item.Props [Style [Cursor "text"]];Navbar.Item.Modifiers [Modifier.TextColor IsGrey] ]
                    [ str "Have an account?" ]
                Navbar.Item.div [ ] [
                    Input.text
                        [ Input.OnChange (
                            fun e ->
                                dispatch (UpdateLoginUsername e.Value)
                            )
                          Input.Placeholder "Username"
                          Input.Value model.LoginModel.Username
                          Input.Props [ Helper.onEnter (DotnetLoginRequest model.LoginModel) dispatch ]
                            ]
                    ]
                Navbar.Item.div [ ][
                    Input.password
                        [ Input.OnChange (
                            fun e ->
                                dispatch (UpdateLoginUserPw e.Value)
                            )
                          Input.Placeholder "Password"
                          Input.Value model.LoginModel.Password
                          Input.Props [ Helper.onEnter (DotnetLoginRequest model.LoginModel) dispatch ]
                            ]
                    ]
                Navbar.Item.a [
                    Navbar.Item.Props [
                        OnClick (fun _ -> dispatch (DotnetLoginRequest model.LoginModel));
                        Style [
                            PaddingLeft "5%" ; PaddingRight "5%";
                            AlignContent AlignContentOptions.Center;
                            BorderRadius "10px"
                            MarginLeft "5%"; MarginRight "5%"; MarginTop "3%"
                        ]
                    ]
                    Navbar.Item.Modifiers [ Modifier.BackgroundColor IsInfo; Modifier.TextColor IsWhite ]
                    ] [
                    Text.p
                        [ Modifiers [ Modifier.TextAlignment (Screen.All,TextAlignment.Centered) ]; Props [ Style [ TextAlign TextAlignOptions.Center; Width "90%" ] ] ]
                        [ str "Login" ]
                    ]
                hr [Class "hrDivider"]
                Navbar.Item.div
                    [ Navbar.Item.Props [Style [Cursor "text";PaddingTop 0]];Navbar.Item.Modifiers [Modifier.TextColor IsGrey] ]
                    [ str "New here?" ]
                Navbar.Item.a [
                    Navbar.Item.Props [
                        OnClick (fun _ -> dispatch (UpdateExtraElement RegisterModal));
                        Style [
                            PaddingLeft "5%" ; PaddingRight "5%";
                            AlignContent AlignContentOptions.Center;
                            BorderRadius "10px"
                            MarginLeft "5%"; MarginRight "5%"; MarginTop "3%"; MarginBottom "7%"
                            Border @"1px solid hsl(204, 86%, 53%)"
                        ]
                    ]
                    Navbar.Item.Modifiers [Modifier.TextColor IsInfo ]
                    ] [
                    Text.p [
                        Modifiers [ Modifier.TextAlignment (Screen.All,TextAlignment.Centered) ];
                        Props [ Style [ TextAlign TextAlignOptions.Center; Width "90%" ] ]
                        ]
                        [ str "Sign Up" ]
                    ]
                hr [Class "hrDivider"]
                div [ Style [ Width "100%"; AlignContent AlignContentOptions.Center ] ] [
                    googleOAuthButton
                ]
                div [ Style [ Width "100%"; AlignContent AlignContentOptions.Center ] ] [
                    githubOAuthButton
                ]
                div [ Style [ Width "100%"; AlignContent AlignContentOptions.Center ] ] [
                    orcidOAuthButton
                ]
            ]
        ]
    ]
    ]

let loggedInNavbar (model : Model) (dispatch : Msg -> unit) =
    [
        Navbar.Item.div [ Navbar.Item.Props [ Style [ MarginLeft "1rem"; MarginRight "0.5rem" ] ] ] [
            Fa.i [
                Fa.Solid.Bars
                Fa.Props [
                    OnClick (fun _ ->  dispatch ToggleMenu)
                    Style [
                        Cursor "pointer"
                    ]
                ]
            ] []
        ]
        Navbar.Item.div
            [ ]
            [ Heading.h2 [ ]
                [ str "SAFE Template - Login" ]
            ]
        Navbar.End.a [ ] [
            Navbar.Item.div [
                Navbar.Item.IsHoverable;
                Navbar.Item.HasDropdown;
            ] [
                Navbar.Link.a [ Navbar.Link.IsArrowless ] [
                    Text.span
                        [ Modifiers [ Modifier.TextWeight TextWeight.SemiBold; Modifier.TextColor Color.IsBlackBis ] ]
                        [ str (if model.User.IsSome then model.User.Value.Username else "No User Information")]
                ]
                Navbar.Dropdown.div [ Navbar.Dropdown.IsRight ] [
                    Navbar.divider [ ] [ ]
                    Navbar.Item.a
                        [ Navbar.Item.Props [OnClick ( fun _ -> dispatch DotnetLogOutRequest)] ]
                        [ str "Logout" ]
                ]
            ]
            Navbar.Item.div [] [br []]
        ]
    ]
