namespace Shared

open System

type Todo = { Id: Guid; Description: string }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos: unit -> Async<Todo list>
      addTodo: Todo -> Async<Todo> }

open IdentityTypes

type IIdentityApi = {
    login       : LoginInfo     -> Async<unit>
    register    : SignupInfo    -> Async<unit>
    getNumTest  : unit          -> Async<int>
}

type IUserApi = {
    getActiveUser       : unit                  -> Async<User>
    updateUserProfile   : User                  -> Async<User>
    /// LoginInfo is used to store current password for additional auth.
    /// String is the new password.
    updatePassword      : LoginInfo * string    -> Async<unit>
    logout              : unit                  -> Async<unit>
    getHelloUser        : unit                  -> Async<string>
}

type IAdminApi = {
    getHelloAdmin       : unit -> Async<string>
    getUsers            : unit -> Async<User []>
}