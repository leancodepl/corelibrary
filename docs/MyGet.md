### Referencing packages

Try not to reference too many NuGets in the core library projects, as this will make versioning harder. There are packages "common" to all infrastructure-related projects, that is:

 - Autofac
 - Serilog

They change frequently, and sometimes we really want them to be up-to-date, so they are in separate `.targets` file - if you want to reference any of them, reference all of them or discuss this with the whole team (i.e. `.targets` file split).

There may be more NuGets that are shared between projects that have not been identified yet. If you find one - report it.

There's also a problem with ASP.NET Core MVC - we want to have it up-to-date, but it has many, many packages that are not required for all of the project. Hence the `Mvc.targets` file that just binds the versions. Each package is responsible for referencing the required set of MVC-related packages.

### Using LeanCode packages in your project

As all of the LeanCode packages are stored in a private Nuget feed, there's some additional configuration required.

#### Per-User configuration
First, you need to add new feed to your global Nuget configuration:

1. Ask for an invitation to the company's MyGet account. Sign up to MyGet using the invitation.
2. Make sure that you have `nuget.exe` command line tool installed on your system (you can download it [here](https://dist.nuget.org/index.html)).
3. Run the code below in the command line:

```
nuget.exe source add -Name LeanCode -Source "https://www.myget.org/F/leancode/api/v3/index.json" -UserName "{your_myget_username}" -Password "{your_myget_password}" -StorePasswordInClearText
nuget.exe setapikey "{your_myget_feed_apikey}" -Source "https://www.myget.org/F/leancode/api/v3/index.json"
```

The last switch (`-StorePasswordInClearText`) is required only for non-windows operating systems.

#### Per-Project configuration
To use LeanCode packages in your project you have to add company's nuget feed to your solution:

1. Make sure that you have `nuget.config` file in your solution directory (or create a new one if you don't)
2. Add new package source (with key 'LeanCode') to your config.

The complete nuget.config file should look like this:

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="LeanCode" value="https://www.myget.org/F/leancode/api/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

Now you can reference LeanCode packages in your project. Check if everything works fine with `dotnet restore` command.
