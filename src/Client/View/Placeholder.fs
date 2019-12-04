module Client.View.Placeholder

open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome

let constructionLabel model dispatch =
    Column.column [ Column.Width (Screen.All,Column.IsHalf); Column.Offset (Screen.All, Column.IsOneQuarter);][
        Box.box' [ Modifiers [Modifier.BackgroundColor Color.IsWhiteTer]; Props[Style[Color "#ff9900"]] ][
            p [Style [TextAlign TextAlignOptions.Center;FontWeight "bold"]][str "ATTENTION YOU ENTERED A LINK THAT IS STILL UNDER CONSTRUCTION!"]
            p [Style [TextAlign TextAlignOptions.Center;FontWeight "bold"]][str "PLEASE COME AGAIN LATER!"]
            div [ Style [Width "100%";MarginTop "2rem"; Display DisplayOptions.Flex; JustifyContent "center" ] ][
                Fa.span [ Fa.Solid.Wrench; Fa.Size Fa.Fa6x;Fa.Props [Style[MarginLeft "1rem";MarginRight "1rem"]]][]
                Fa.span [ Fa.Solid.Tools; Fa.Size Fa.Fa6x;Fa.Props [Style[MarginLeft "1rem";MarginRight "1rem"]]][]
                Fa.span [ Fa.Solid.PaintRoller; Fa.Size Fa.Fa6x;Fa.Props [Style[MarginLeft "1rem";MarginRight "1rem"]] ][]
            ]
        ]
    ]

let welcomeElement model dispatch =
    Hero.hero [
        Hero.Props [
            Style [
                BackgroundImage @"linear-gradient(rgba(0, 0, 0, 0.0), rgba(0, 0, 0, 0.0)), url('https://www.tokkoro.com/picsup/2602833-minimalism-wallpaper-hd-windows.jpg')"
                BackgroundPosition "center"
                BackgroundSize "contain"
            ]
        ]
    ] [
        Hero.head [ ] [ ]
        Hero.body [ ] [
            Container.container [
                Container.IsFluid
                Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ]
            ] [
            Heading.h1 [ ] [ str "ExampleApp" ]
            Heading.h2 [ Heading.IsSubtitle ] [ str "User Manager" ]
            constructionLabel model dispatch
            ]
        ]
    ]
