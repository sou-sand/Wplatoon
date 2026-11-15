# Data feeds and caching

Wplatoon reads public JSON feeds maintained by the Splatoon.ink projects. It does not own those feeds, and their structure or availability can change independently of this repository.

## Feeds used by the app

| Game or content | Feed |
| --- | --- |
| Splatoon 3 schedules | `https://splatoon3.ink/data/schedules.json` |
| Splatoon 3 shop gear | `https://splatoon3.ink/data/gear.json` |
| Splatoon 3 Salmon Run and monthly reward | `https://splatoon3.ink/data/coop.json` |
| Splatoon 3 Splatfests | `https://splatoon3.ink/data/festivals.json` |
| Splatoon 2 schedules | `https://splatoon2.ink/data/schedules.json` |
| Splatoon 2 Salmon Run | `https://splatoon2.ink/data/coop-schedules.json` |

The endpoints are selected in `AppSettings.cs`. The corresponding parsing is split between `ScheduleParser.cs` for Splatoon 3 schedules, `Splatoon2Parser.cs` for Splatoon 2, and `GearParser.cs` for Splatoon 3 gear, rewards, and Splatfests.

Splatfest records are selected using the region code from settings. Schedule timestamps are converted to the phone's local time for display. Parsers ignore rotations that have already ended and skip items whose required fields are missing.

## When data is requested

The home page loads the selected game's schedules when it opens. The Splatoon 3 gear and co-op feeds are loaded in Splatoon 3 mode; the Splatoon 2 co-op feed is loaded in Splatoon 2 mode. The Splatfest feed is deferred until the Splatfest section is opened.

Each feed has its own cache file in the app's isolated storage. `CacheHelper.MaxAge` is one hour. Wplatoon can display a valid cached response while it asks for a newer one, and can continue showing that saved response when a request fails. A cache is a convenience, not a guarantee of up-to-date schedules.

The foreground app sends the contact-bearing user-agent string defined in `AppSettings.UserAgent`. Please keep the upstream projects' policies in mind when changing request behavior:

- [Splatoon 3.ink data access](https://github.com/misenhower/splatoon3.ink/wiki/Data-Access)
- [Splatoon 2.ink data-access policy](https://github.com/misenhower/splatoon2.ink/wiki/Data-access-policy)

## Wallpaper and tile images

The wallpaper page combines bundled images with stage images found in cached schedule responses. Choosing a remote wallpaper downloads it to isolated storage before asking Windows Phone to use it as the lock screen.

The foreground tile helper downloads a stage image when needed and updates the app's pinned tile. The separate scheduled agent has its own caching logic and is not currently a dependable route for background tile updates; see [the agent notes](./wplatoon-agent.md).
