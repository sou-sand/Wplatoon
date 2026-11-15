# How the Wplatoon app works

Wplatoon is a Windows Phone 8 Silverlight app written in C# and XAML. The phone app owns the screens and most of the data work. It does not run its own web service or use a server-side database.

## The everyday flow

When the home page opens, it selects the currently chosen game and loads schedule data. It first looks in the phone's isolated storage. If valid saved data exists, it can show that data immediately; when the saved copy is old, it also requests a fresh copy from the upstream feed.

The response is parsed into `ItemViewModel` objects, which the XAML lists display. Tapping one stores the selected item in `DataSource` and navigates to the detail page. The home page updates its countdown once per second and asks the tile helper to refresh the pinned tile when schedules are displayed.

Splatoon 3 gear and Salmon Run data are loaded with the schedule data. Splatfest data is requested only when the Splatfest section is opened. Splatoon 2 mode uses its own schedule and Salmon Run feeds.

## Main pieces of the app

| Area | Files | Responsibility |
| --- | --- | --- |
| App startup and navigation | `App.xaml`, `App.xaml.cs` | Starts the phone application and its navigation frame |
| Home screen | `HomePage.xaml`, `HomePage.xaml.cs` | Selects a game, fetches data, shows lists, refreshes, and counts down |
| Details and reminders | `DetailPage.xaml`, `DetailPage.xaml.cs` | Shows the selected item and creates phone reminders |
| Settings | `SettingsPage.xaml`, `SettingsPage.xaml.cs`, `AppSettings.cs` | Saves game, clock, region, language, and feed choices |
| Data conversion | `ScheduleParser.cs`, `Splatoon2Parser.cs`, `GearParser.cs`, `ItemViewModel.cs` | Turns the two feed formats into items the UI can display |
| Local data | `CacheHelper.cs` | Reads and writes feed responses in isolated storage |
| Pinned tile | `TileHelper.cs`, `TileUpdater.cs` | Builds the foreground app's tile from a current schedule and stage image |
| Wallpapers | `WallpapersPage.xaml`, `WallpapersPage.xaml.cs` | Lists packaged and cached-feed stage artwork and requests lock-screen access |

`Wplatoon.sln` contains both the main app project (`Wplatoon/Wplatoon.csproj`) and the separate agent project (`WplatoonAgent/WplatoonAgent/WplatoonAgent.csproj`). The agent is not the same thing as the app's foreground tile update code; see [WplatoonAgent](./wplatoon-agent.md).

## Settings and saved data

`AppSettings` reads and writes user choices through `IsolatedStorageSettings`. Feed responses are stored as files through `CacheHelper`, with a saved timestamp alongside each file. The details of the feeds and freshness rule are in [Data and caching](./data-and-caching.md).

The data model is intentionally small: parsers create display-ready items, and the pages bind those items to lists. If an upstream feed changes shape, the relevant parser and the screen that consumes it are the places to investigate.

## Older platform trade-offs

The app uses APIs and controls from the Windows Phone 8 Silverlight platform. Network work uses `WebClient`, local files use isolated storage, and reminders and tile updates go through phone-specific APIs. That is why this project cannot be treated like a current .NET or UWP app without a deliberate migration.
