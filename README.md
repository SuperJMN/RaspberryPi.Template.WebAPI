## A Raspberry Pi Template for ASP.NET Core 2 Web APIs
An empty project template for .NET Core 2 IoT applications deployed to the Raspberry Pi.

## How to install the template

```
dotnet new -i RaspberryPi.Template.WebApi
```

## How to create a project with the template
Like always! With this command

```
dotnet new rpiwebapi -n [ProjectName]
```

It creates an **ASP.NET Core 2.0 Web API** application with
- **Swagger** configured
- **Logging**

Why them? Because most Web APIs use these features :)

## How to build + deploy?
Terribly easy!

- Open build.cake and replace the first lines of the file with your parameters. 
- Create an folder in your Raspberry Pi according to the deployment path (`/home/pi/DotNet/[YourAppName]`)
- From the project folder run the build.ps1 script from **PowerShell**

**NOTICE:** The build script requires [PuTTY](https://putty.org/) to be installed. It's use to transfer files to the Raspberry Pi using SSH.
