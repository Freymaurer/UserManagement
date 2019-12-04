module Client.View.Counter

open Fable.React
open Fable.React.Props
open Fulma
open Shared

open Client.Types

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let show model =
    match model with
    | { Counter = Some counter;Loading = false } -> string counter.Value
    | _ -> "Loading..."

let counter model dispatch =
      Container.container [ Container.Props [ Style [MarginTop "1rem"] ] ] [
        Column.column [Column.Modifiers [Modifier.TextAlignment (Screen.All,TextAlignment.Centered)]][
            Heading.h6 [] [ str "Welcome! This is currently a placeholder Welcome-Screen. Please login to access user management functions." ]
        ]
        Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
           [ Heading.h3 [] [ str ("Press buttons to manipulate counter: " + show model) ] ]
        Columns.columns []
           [ Column.column [] [ button "-" (fun _ -> dispatch Decrement) ]
             Column.column [] [ button "+" (fun _ -> dispatch Increment) ]
             Column.column [] [ button "secret" (fun _ -> dispatch GetUserCounterRequest) ] ] ]