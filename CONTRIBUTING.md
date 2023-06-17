Bloodmasters Contributing
=========================

Build
-----
### Prerequisites
- [Visual Studio 2022][visual-studio]
- [netfx4sdk][]

### Commands
First, apply the netfx4sdk workaround (only need to do once):
```console
# netfx43sdk.cmd -mode sys
```

Then, build the solutions using Visual Studio (in the **Developer Command Prompt**):
```
$ devenv.exe Source\Launcher.sln /Build
```

[visual-studio]: https://visualstudio.microsoft.com/vs/
[netfx4sdk]: https://github.com/3F/netfx4sdk
