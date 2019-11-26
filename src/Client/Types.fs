module Client.Types

open System

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Fulma
open Thoth.Json
open Fable.Core
open Shared
open Fable.FontAwesome

let [<Literal>] ENTER_KEY = 13.

type MainReactElement =
| Welcome
| Counter
| UserAccount of User
| UserList

type ExtraReactElement =
|EmptyElement
|RegisterModal
|VerifyLoginModal of Msg * (Model -> (Msg -> unit) -> ReactElement)
|AddUsernameToExternLoginModal
|AdminRegisterModal
|Message of string

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
and Model = {
    Counter: Counter option
    ErrorMsg : string option
    InputString : string
    LoginModel : LoginModel
    RegisterModel : RegisterModel
    User : User option
    Loading : bool
    Authenticated : bool
    ExtraReactElement : ExtraReactElement
    MainReactElement : MainReactElement
    ShowMenuBool : bool
    AdminUserList : User []
    AdminUserListRoleFilter : ActiveUserRoles
    AdminViewUser : User option
    AdminAssignRole : ActiveUserRoles
    }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
and Msg =
    | ClearRegisterLogin
    | ToggleMenu
    | ChangeMainReactElement of MainReactElement
    | SortAllUserList of string
    | FilterAllUserList of ActiveUserRoles
    | AdminSelectUser of User
    | AdminSelectAssignRole of ActiveUserRoles
    | Increment
    | Decrement
    | UpdateInputString of string
    | InitialCountLoaded of Counter
    | InitialUserLoaded of User
    | UpdateLoginUsername of string
    | UpdateLoginUserPw of string
    | UpdateRegisterModel of RegisterModel
    | UpdateExtraElement of ExtraReactElement
    | Debug of string
    | GetTestRequest of string
    | GetTestResponse of Result<string, exn>
    | DotnetRegisterRequest of RegisterModel
    | DotnetRegisterResponse of Result<DotnetRegisterResults,exn>
    | DotnetLoginRequest of LoginModel
    | DotnetLoginResponse of Result<DotnetLoginResults,exn>
    | DotnetGetUserRequest
    | DotnetGetUserResponse of Result<User,exn>
    | DotnetLogOutRequest
    | DotnetLogOutResponse of Result<DotnetLogOutResults,exn>
    | DotnetChangeUserParamRequest of LoginModel * UserParameters * string
    | DotnetChangeUserParamResponse of Result<DotnetChangeParameterResults,exn>
    | GetUserCounterRequest
    | GetUserCounterResponse of Result<Counter,exn>
    | DeleteAccountRequest of LoginModel
    | DeleteAccountResponse of Result<DotnetDeleteAccountResults,exn>
    | AdminGetAllUsersRequest
    | AdminGetAllUsersResponse of Result<User [],exn>
    | AdminRegisterUserRequest of RegisterModel * ActiveUserRoles
    | AdminRegisterUserResponse of Result<DotnetRegisterResults,exn>
    | AdminChangeUserParamsRequest of LoginModel * User * UserParameters * string
    | AdminChangeUserParamsResponse of Result<DotnetChangeParameterResults,exn>
    | AdminDeleteAccountRequest of LoginModel * User
    | AdminDeleteAccountResponse of Result<DotnetDeleteAccountResults,exn>
    | GetContextClaimsRequest
    | GetContextClaimsResponse of Result<string,exn>
    | AddUsernameToExtLogin of string
    | AddUsernameToExtLoginResponse of Result<string,exn>
    | GetExternalLoginTest of string*string
    | GetExternalLoginTestResponse of Result<string,exn>

module ServerPath =

    /// when publishing to IIS, your application most likely runs inside a virtual path (i.e. localhost/SafeApp)
    /// every request made to the server will have to account for this virtual path
    /// so we get the virtual path from the location
    /// `virtualPath` of `http://localhost/SafeApp` -> `/SafeApp/`
    [<Emit("window.location.pathname")>]
    let virtualPath : string = jsNative

    /// takes path segments and combines them into a valid path
    let combine (paths: string list) =
        paths
        |> List.map (fun path -> List.ofArray (path.Split('/')))
        |> List.concat
        |> List.filter (fun segment -> not (segment.Contains(".")))
        |> List.filter (String.IsNullOrWhiteSpace >> not)
        |> String.concat "/"
        |> sprintf "/%s"

    /// Normalized the path taking into account the virtual path of the server
    let normalize (path: string) = combine [virtualPath; path]

module Server =

    open Fable.Remoting.Client

    // normalize routes so that they work with IIS virtual path in production
    let normalizeRoutes typeName methodName =
        Route.builder typeName methodName
        |> ServerPath.normalize

    /// A proxy you can use to talk to server directly
    let userApi : IUserApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder normalizeRoutes
        |> Remoting.buildProxy<IUserApi>

    let dotnetSecureApi : IDotnetSecureApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder normalizeRoutes
        |> Remoting.buildProxy<IDotnetSecureApi>

    let dotnetAdminSecureApi: IAdminSecureApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder normalizeRoutes
        |> Remoting.buildProxy<IAdminSecureApi>