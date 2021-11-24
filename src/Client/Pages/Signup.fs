module Signup

open Elmish

open Model
open Messages.Signup
open Messages
open Feliz
open Feliz.Bulma
open Fable.React

let init() : Signup.Model * Cmd<Msg> =
    let m = Signup.Model.init
    m, Cmd.none

let update (msg: Signup.Msg) (model:Model) (state: Signup.Model) : Model * Cmd<Msg> =
    match msg with
    | UpdateSignupInfo register ->
        let nextState = {state with SignupInfo = register}
        let nextModel = { model with PageModel = PageModel.Signup nextState}
        nextModel, Cmd.none
    | UpdatePasswordDuplicate duplicate ->
        let nextState = {state with PasswordDuplicate = duplicate}
        let nextModel = { model with PageModel = PageModel.Signup nextState}
        nextModel, Cmd.none 

let mainElement (model:Model) (state:Signup.Model) dispatch =
    Bulma.box [
        Bulma.field.div [
            Bulma.label "Email"
            Bulma.control.div [
                Bulma.input.email [
                    // https://stackoverflow.com/questions/37443993/reactjs-value-of-previous-input-is-set-by-default-in-new-input
                    prop.id "signup_email"
                    prop.key "key_signup_email"
                    prop.placeholder "email@provider.de"
                    prop.onKeyDown(fun e ->
                        match e.key with
                        | "Enter" ->
                            e.stopPropagation()
                            e.preventDefault()
                            Identity.SignupRequest state.SignupInfo |> IdentityMsg |> dispatch
                        | _ -> ()
                    )
                    prop.onChange (fun (e:Browser.Types.Event) ->
                        let info = { state.SignupInfo with Email = e.Value }
                        UpdateSignupInfo info |> SignupMsg |> dispatch
                    )
                ]
            ]
        ]
        Bulma.field.div [
            Bulma.label "Username"
            Bulma.control.div [
                Bulma.input.text [
                    prop.id "signup_username"
                    prop.key "key_signup_username"
                    prop.placeholder "username"
                    prop.onKeyDown(fun e ->
                        match e.key with
                        | "Enter" ->
                            e.stopPropagation()
                            e.preventDefault()
                            Identity.SignupRequest state.SignupInfo |> IdentityMsg |> dispatch
                        | _ -> ()
                    )
                    prop.onChange (fun (e:Browser.Types.Event) ->
                        let info = { state.SignupInfo with Username = e.Value }
                        UpdateSignupInfo info |> SignupMsg |> dispatch
                    )
                ]
            ]
        ]
        Bulma.field.div [
            Bulma.label "Password"
            Bulma.control.div [
                Bulma.input.password [
                    prop.id "signup_password"
                    prop.key "key_signup_password"
                    prop.placeholder "*****"
                    prop.onKeyDown(fun e ->
                        match e.key with
                        | "Enter" ->
                            e.stopPropagation()
                            e.preventDefault()
                            Identity.SignupRequest state.SignupInfo |> IdentityMsg |> dispatch
                        | _ -> ()
                    )
                    prop.onChange (fun (e:Browser.Types.Event) ->
                        let info = { state.SignupInfo with Password = e.Value }
                        UpdateSignupInfo info |> SignupMsg |> dispatch
                    )
                ]
            ]
        ]
        Bulma.field.div [
            Bulma.label "Repeat Password"
            Bulma.control.div [
                Bulma.input.password [
                    prop.id "signup_email_duplicate"
                    prop.key "key_signup_email_duplicate"
                    prop.placeholder "*****"
                    prop.onKeyDown(fun e ->
                        match e.key with
                        | "Enter" ->
                            e.stopPropagation()
                            e.preventDefault()
                            Identity.SignupRequest state.SignupInfo |> IdentityMsg |> dispatch
                        | _ -> ()
                    )
                    prop.onChange (fun (e:Browser.Types.Event) ->
                        let duplicate = e.Value 
                        UpdatePasswordDuplicate duplicate |> SignupMsg |> dispatch
                    )
                ]
            ]
            if state.PasswordDuplicate <> "" && state.PasswordDuplicate <> state.SignupInfo.Password then
                Bulma.help [
                    color.hasTextDanger
                    prop.text "Passwords are different."
                ]
        ]
        Bulma.field.div [
            Bulma.field.isGrouped
            Bulma.field.isGroupedCentered
            prop.children [
                Bulma.control.div [
                    Bulma.button.button [
                        let passwordsAreDifferent = state.SignupInfo.Password <> state.PasswordDuplicate
                        let hasMissingInfo = state.SignupInfo.Password = "" || state.SignupInfo.Username = "" || state.SignupInfo.Email = ""
                        if hasMissingInfo || passwordsAreDifferent then Bulma.button.isStatic
                        Bulma.color.isLink
                        prop.text "Submit"
                        prop.onClick(fun _ ->
                            Identity.SignupRequest state.SignupInfo |> IdentityMsg |> dispatch
                        )
                    ]
                ]
            ]
        ]
    ]