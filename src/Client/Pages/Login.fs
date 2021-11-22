module Login

open Elmish

open Model
open Messages.Login
open Messages
open Feliz
open Feliz.Bulma
open Fable.React

let update (msg: Login.Msg) (model:Model) (loginmodel: Login.Model) : Model * Cmd<Msg> =
    match msg with
    | UpdateLoginInfo login ->
        let nextLoginModel = {loginmodel with LoginInfo = login}
        let nextModel = { model with PageModel = PageModel.Login nextLoginModel}
        nextModel, Cmd.none

let mainElement (model:Model) (loginModel:Login.Model) dispatch =
    Bulma.box [
        Bulma.field.div [
            Bulma.label "Username"
            Bulma.control.div [
                Bulma.input.text [
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
    ]