module PasswordModal


open Model
open Feliz
open Feliz.Bulma
open Messages
open Fable.FontAwesome
open Fable.React
open Feliz

let passwordConfirmModal (model:Model) dispatch =
    Bulma.modal [
        prop.id "modal-passwordConfirm"
        Bulma.modal.isActive
        prop.children [
            Bulma.modalBackground [
                prop.onClick (fun _ -> UpdatePasswordModal None |> dispatch)
            ]
            Bulma.modalContent [
                Bulma.box [
                    Bulma.field.div [
                        Bulma.label "Username"
                        Bulma.control.div [
                            Bulma.input.text [
                                prop.readOnly true
                                prop.value model.UserState.User.Value.Username
                            ]
                        ]
                    ]
                    Bulma.field.div [
                        Bulma.label "Password"
                        Bulma.control.div [
                            Bulma.input.password [
                                prop.placeholder "*****"
                                prop.defaultValue model.UserState.PasswordModalPw
                                prop.onChange(fun (e:Browser.Types.Event) ->
                                    UpdatePasswordModalPw e.Value |> dispatch
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
                                    let isNotEmpty = model.UserState.PasswordModalPw <> ""
                                    if not isNotEmpty then Bulma.button.isStatic
                                    Bulma.color.isLink
                                    prop.text "Submit"
                                    prop.onClick(fun _ ->
                                        let loginInfo = IdentityTypes.LoginInfo.create model.UserState.User.Value.Username model.UserState.PasswordModalPw
                                        model.UserState.ShowPasswordModal.Value loginInfo |> dispatch
                                    )
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            Bulma.modalClose [
                prop.onClick (fun _ -> UpdatePasswordModal None |> dispatch)
            ]
        ]
    ]