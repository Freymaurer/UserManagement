# SAFE Template

Highly recommend improving error handling and password prompts in this example repo with [Feliz.SweetAlert](https://github.com/Shmew/Feliz.SweetAlert) style alerts.

This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites

You'll need to install the following pre-requisites in order to build this SAFE application.

* [.NET Core SDK](https://www.microsoft.com/net/download) 5.0 or higher
* [Node LTS](https://nodejs.org/en/download/)
* [Docker-compose](https://docs.docker.com/compose/install/)

## Starting the application

Before you run the project **for the first time only** you must install dotnet "local tools" with this command:

```bash
dotnet tool restore
```

Starting the application for the first time will prompt you to install local certificates for `https` support.
When the following image pops up go `Install Certificate` > `Current User` > `Place all certificates in the following store` > `Trusted Root Certification Authorities` > confirm all choices "Finish, yes, etc".

![local certificate img](https://i.stack.imgur.com/igvUim.png)


To concurrently run the server and the client components in watch mode use the following command. This will also run docker-compose for an mssql database (`Port:1433`) and an adminer instance (`Port:8082`). 

!! **Before** you are able to use the auth. functionality you first need to migrate the identityDb schema onto the freshly created docker database. To do this publish the `db/.dbframe/dbframe.dacpac` file to the database. If you know how to automate this step. Please feel free to open an issue with the relevant information or -best case- open a pull request with the solution.

[Publish dacpac with Visual Studio](https://blogs.msmvps.com/deborahk/deploying-a-dacpac-with-visual-studio/)

```bash
dotnet run
```

Then open `http://localhost:8080` in your browser.

The build project in root directory contains a couple of different build targets. You can specify them after `--` (target name is case-insensitive).

To run concurrently server and client tests in watch mode (you can run this command in parallel to the previous one in new terminal):

```bash
dotnet run -- RunTests
```

Client tests are available under `http://localhost:8081` in your browser and server tests are running in watch mode in console.

Finally, there are `Bundle` and `Azure` targets that you can use to package your app and deploy to Azure, respectively:

```bash
dotnet run -- Bundle
dotnet run -- Azure
```

## SAFE Stack Documentation

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
