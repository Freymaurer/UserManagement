# SAFE Template

This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you can also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## Work with the application

Before you run the project **for the first time only** you should install its local tools with this command:

```bash
dotnet tool restore
```


To concurrently run the server and the client components in watch mode use the following command:

```bash
dotnet fake build -t run
```

### Authentication and Authorization

This example app for fsharp UserManagement in a [SAFE](https://safe-stack.github.io/docs/intro/) stack environmant is originally based on [ASP.NET Core](https://docs.microsoft.com/de-de/aspnet/core/security/?view=aspnetcore-3.1), This is
wrapped in a functional first approach by the F# web framework [Giraffe](https://github.com/giraffe-fsharp/Giraffe/blob/master/DOCUMENTATION.md#authentication-and-authorization). Ob top of both is the [Saturn](https://saturnframework.org/explanations/pipeline.html)
library built. This provides an additonal set of optional abstractions which make configuring web applications and constructing complex routes easy to achieve.

So what is nshow in this example is how to use/access cookie-based ASP.NET Authentication and Authorization functions from Giraffe and Saturn. The basic functions necessary to manage user logins and more are included in 'AspNetCoreIdentity.fs' These feature for example:

- Creating and deleting accounts
- Changing Account information
- Admin based change of account information
- OAuth for Google, GitHub and Orcid

In addition this repo can give an idea on how to use Fable.Remoting in combination with Authentication and Authorization.


### Use Asp.Net Identity with an Sql Database

you can create an c# mvc project to create the identy database necessary for the ASP.NET core identity model. To do this add a new 'ASP.NET Core Web Application' to this solution with the template web application (mvc-controlled). Remember to change authentification to 'Individual user accounts'.
This dummy project is used to create the databases used by the Identity framework. To accomplish this, open the solution in Visual Studio -> Tools -> Nuget Package Manager -> Package Manager Console und navigate to the EFIdentityDummyProject - folder (use `dir` to check current location and `cd ThisCouldBeYourPath\EFIdentityDummyProject` to navigate to the folder containing the Startup.cs file) then type `Update-Database -Project EFIdentityDummyProject -StartupProject EFIdentityDummyProject` and the necessary databases will be build. 
([source](https://docs.microsoft.com/de-de/ef/core/miscellaneous/cli/powershell)). If you want to create an IdentityDB on an external SQL-Server you need to change the connectionstring 'DefaultConnection' at '\DummyProject\appsettings.json' to the connectionstring of your SQL-server and then update the database as described earlier.

## SAFE Stack Documentation

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/docs/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
* [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/)
* [Fulma](https://fulma.github.io/Fulma/)

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).
