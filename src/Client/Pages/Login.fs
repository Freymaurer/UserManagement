module Login

open Elmish

open Model
open Messages.Login
open Messages
open Feliz.Bulma
open Fable.React
open Feliz

let init() : Login.Model * Cmd<Msg> =
    let m = Login.Model.init
    m, Cmd.none

let update (msg: Login.Msg) (model:Model) (state: Login.Model) : Model * Cmd<Msg> =
    match msg with
    | UpdateLoginInfo login ->
        let nextState = {state with LoginInfo = login}
        let nextModel = { model with PageModel = PageModel.Login nextState}
        nextModel, Cmd.none

let private githubOAuthButton (model:Model) dispatch =
    Bulma.field.div [
        Bulma.button.a [
            prop.href StaticStrings.OAuthPaths.GithubOAuth
            Bulma.button.isFullWidth
            color.isBlack
            prop.children [
                Html.span "Log In with GitHub"
                Html.i [prop.className "fab fa-github fa-lg fa-pull-right"]
            ]
        ]
    ]

let private googleOAuthButton (model:Model) dispatch =
    Bulma.field.div [
        Bulma.button.a [
            prop.href StaticStrings.OAuthPaths.GoogleOAuth
            Bulma.button.isFullWidth
            color.isInfo
            prop.children [
                Html.span "Log In with Google"
                Html.i [prop.className "fab fa-google fa-lg fa-pull-right"]
            ]
        ]
    ]

let private orcidOAuthButton (model:Model) dispatch =
    Bulma.field.div [
        Bulma.button.a [
            prop.href StaticStrings.OAuthPaths.OrcidOAuth
            prop.disabled true
            prop.title "Have not updated redirect url for orcid oauth. Logic should work, if you want to test it set up your own orcid oauth settings and replace id and key in Server.fs."
            Bulma.button.isFullWidth
            prop.style [style.color "#a9c518"]
            prop.children [
                Html.span "Log In with Orcid"
                Html.i [prop.className "fab fa-orcid fa-lg fa-pull-right"]
            ]
        ]
    ]

let mainElement (model:Model) (loginModel:Login.Model) dispatch =
    Bulma.box [
        Bulma.field.div [
            Bulma.label "Username"
            Bulma.control.div [
                Bulma.input.text [
                    prop.id "login_username"
                    prop.placeholder "username"
                    prop.onKeyDown(fun e ->
                        match e.key with
                        | "Enter" ->
                            e.stopPropagation()
                            e.preventDefault()
                            Identity.LoginRequest loginModel.LoginInfo |> IdentityMsg |> dispatch
                        | _ -> ()
                    )
                    prop.onChange (fun (e:Browser.Types.Event) ->
                        let info = { loginModel.LoginInfo with Username = e.Value }
                        UpdateLoginInfo info |> LoginMsg |> dispatch
                    )
                ]
            ]
        ]
        Bulma.field.div [
            Bulma.label "Password"
            Bulma.control.div [
                Bulma.input.password [
                    prop.id "login_password"
                    prop.placeholder "*****"
                    prop.onKeyDown(fun e ->
                        match e.key with
                        | "Enter" ->
                            e.stopPropagation()
                            e.preventDefault()
                            Identity.LoginRequest loginModel.LoginInfo |> IdentityMsg |> dispatch
                        | _ -> ()
                    )
                    prop.onChange (fun (e:Browser.Types.Event) ->
                        let info = { loginModel.LoginInfo with Password = e.Value }
                        UpdateLoginInfo info |> LoginMsg |> dispatch
                    )
                ]
            ]
        ]
        Bulma.field.div [
            Bulma.field.isGrouped
            Bulma.field.isGroupedCentered
            prop.children [
                Bulma.control.div [
                    Bulma.button.button [
                        let hasMissingInfo = loginModel.LoginInfo.Password = "" || loginModel.LoginInfo.Username = ""
                        if hasMissingInfo then Bulma.button.isStatic
                        Bulma.color.isLink
                        prop.text "Submit"
                        prop.onClick(fun _ ->
                            Identity.LoginRequest loginModel.LoginInfo |> IdentityMsg |> dispatch
                        )
                    ]
                ]
            ]
        ]
        Bulma.Divider.divider [prop.text "OR"]
        githubOAuthButton model dispatch
        googleOAuthButton model dispatch
        orcidOAuthButton model dispatch
    ]