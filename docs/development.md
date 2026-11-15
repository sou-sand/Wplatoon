# Building and contributing

Thanks for taking an interest in Wplatoon. It is a small, volunteer project built for a platform that is no longer actively supported, so the first challenge is usually getting the original toolchain running.

## Build requirements

The projects target **Windows Phone 8 Silverlight** and use the legacy phone SDK and Windows Phone controls. The repository was created with Visual Studio 2015-era project files. A compatible Windows setup with the Windows Phone 8 SDK is needed; current Visual Studio releases may not install or support that SDK.

To try a build:

1. Open `Wplatoon.sln` in a compatible Visual Studio installation.
2. Let Visual Studio restore the NuGet packages if needed.
3. Build the solution, then deploy the `Wplatoon` app to an available Windows Phone device or emulator target.

The solution contains two projects: the user-facing app and `WplatoonAgent`. The agent project has known incomplete code; a successful app build should not be taken as proof that the scheduled agent works. Check [the agent notes](./wplatoon-agent.md) if you are changing that area.

## Making a change

1. Keep the change focused on one user-visible improvement or code issue.
2. Follow the existing C# and XAML patterns; this codebase predates modern .NET conventions.
3. For feed changes, check the source policy first and update the parser that owns that feed format.
4. Build with the legacy toolchain when available, and manually exercise the affected screen or phone behavior.
5. Describe what you tried in your pull request, including any platform or emulator limitation.

There is no automated test project in the solution at present. Do not claim a behavior was verified on a device unless it actually was.

## Where to start

- For screen behavior, start in `HomePage.xaml.cs`, `DetailPage.xaml.cs`, or the page named by the feature.
- For feed interpretation, look at the matching parser and `ItemViewModel.cs`.
- For settings or cached files, start with `AppSettings.cs` and `CacheHelper.cs`.
- For live tiles, distinguish the foreground app's `TileUpdater.cs` from the separate `WplatoonAgent` project.

Please also read the project [license](../LICENSE) and the data providers' access notes linked from [Data and caching](./data-and-caching.md).
