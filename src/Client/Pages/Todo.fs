module Todo

open Model
open Messages.Todo
open Messages

open Feliz
open Feliz.Bulma
open Shared
open Elmish

let update (msg: Todo.Msg) (model:Model) (todomodel: Todo.Model) : Model * Cmd<Msg> =
    match msg with
    | GotTodos todos ->
        let todoModel = { todomodel with Todos = todos }
        let nextModel = { model with PageModel = PageModel.Home todoModel}
        nextModel, Cmd.none
    | SetInput value ->
        let todoModel = { todomodel with Input = value }
        let nextModel = { model with PageModel = PageModel.Home todoModel}
        nextModel, Cmd.none
    | AddTodo ->
        let todo = Todo.create todomodel.Input

        let cmd =
            Cmd.OfAsync.perform Api.todosApi.addTodo todo AddedTodo
            |> Cmd.map TodoMsg

        let todoModel = { todomodel with Input = "" }
        let nextModel = { model with PageModel = PageModel.Home todoModel}
        nextModel, cmd
    | AddedTodo todo ->
        let todoModel = { todomodel with Todos = todomodel.Todos @ [ todo ] }
        let nextModel = { model with PageModel = PageModel.Home todoModel}
        nextModel, Cmd.none

let mainElement (model: Model) (todoModel:Todo.Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.content [
            Html.ol [
                for todo in todoModel.Todos do
                    Html.li [ prop.text todo.Description ]
            ]
        ]
        Bulma.field.div [
            field.isGrouped
            prop.children [
                Bulma.control.p [
                    control.isExpanded
                    prop.children [
                        Bulma.input.text [
                            prop.value todoModel.Input
                            prop.placeholder "What needs to be done?"
                            prop.onChange (fun x -> SetInput x |> TodoMsg |> dispatch)
                        ]
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isPrimary
                        prop.disabled (Todo.isValid todoModel.Input |> not)
                        prop.onClick (fun _ -> dispatch (TodoMsg AddTodo))
                        prop.text "Add"
                    ]
                ]
            ]
        ]
    ]