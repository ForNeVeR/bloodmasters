Bloodmasters Contributing
=========================

Build
-----
### Prerequisites
- Windows OS (for now)
- [.NET 10 SDK][dotnet-sdk] or a later compatible version

### Commands
Build the solutions using the following shell command:
```
$ dotnet build Source/Bloodmasters.sln
```

Debug
-----
By default, the game client falls back to running a launcher if started without command line arguments. This behavior makes it harder to debug the client, so there's a special debugging configuration available.

To start the game client under local debugger, use the **Standalone** profile of `launchSettings.json` file in the `Bloodmasters` project. It will load the client without the need for launcher, using the default debug configuration files.

If you are okay with the default behavior (starting the launcher), run the profile named **Default**.

[dotnet-sdk]: https://dot.net
