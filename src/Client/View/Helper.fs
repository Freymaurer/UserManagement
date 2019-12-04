module Client.View.Helper

open Fable.React
open Fable.React.Props
open Fulma

open Client.Types

let onEnter msg dispatch =
    OnKeyDown (fun ev ->
        if ev.keyCode = ENTER_KEY then
            dispatch msg)

let emptyStr = str ""

let messageContainer (content:string) msg =
    Container.container [ Container.Props [Style [MarginTop "2%"]]] [
        Columns.columns [ Columns.IsCentered ][
            Column.column [ Column.Width (Screen.All,Column.IsHalf) ][
                Content.content [
                    Content.Modifiers [Modifier.TextColor IsDanger;Modifier.TextAlignment (Screen.All,TextAlignment.Centered)]
                    Content.Props [
                        Style [
                            MarginTop "1%"; MarginBottom "2%"
                        ]
                    ]
                ] [
                    str content
                ]
            ]
            Column.column [ Column.Width (Screen.All,Column.Is1) ][
                Delete.delete [ Delete.OnClick msg ][]
            ]
        ]
    ]