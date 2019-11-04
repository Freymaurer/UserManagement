# SAFE Template

This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download)
* [FAKE 5](https://fake.build/) installed as a [global tool](https://fake.build/fake-gettingstarted.html#Install-FAKE)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you an also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## Additions to SAFE-Template

### Added Dependencies to work with Asp.Net Identity Framework and EntityFramework

```
nuget Microsoft.AspNetCore.Identity.EntityFrameworkCore
nuget Microsoft.AspNetCore.Identity.UI 3.0.0
nuget Microsoft.EntityFrameworkCore.InMemory
nuget Microsoft.EntityFrameworkCore.SqlServer
nuget Microsoft.EntityFrameworkCore.Tools 3.0.0
```

This allows to either use and InMemory storage of users or to use an Sql Server. If you don't want to use the Sql Server you can delete the EFIdentityDummyProject. 

### Use Asp.Net Identity with an Sql Database

The EFIdentityDummyProject is used to create the databases used by the Identity framework. To accomplish this, open the solution in Visual Studio -> Tools -> Nuget Package Manager -> Package Manager Console und navigate to the EFIdentityDummyProject - folder (use `dir` to check current location and `cd ThisCouldBeYourPath\EFIdentityDummyProject` to navigate to the folder containing the Startup.cs file) then type `Update-Database -Project EFIdentityDummyProject -StartupProject EFIdentityDummyProject` and the necessary databases will be build. 
([source](https://docs.microsoft.com/de-de/ef/core/miscellaneous/cli/powershell))

## Work with the application

To concurrently run the server and the client components in watch mode use the following command:

```bash
fake build -t Run
```


## SAFE Stack Documentation

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/docs/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
* [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/)
* [Fulma](https://fulma.github.io/Fulma/)

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

## Troubleshooting

* **fake not found** - If you fail to execute `fake` from command line after installing it as a global tool, you might need to add it to your `PATH` manually: (e.g. `export PATH="$HOME/.dotnet/tools:$PATH"` on unix) - [related GitHub issue](https://github.com/dotnet/cli/issues/9321)
