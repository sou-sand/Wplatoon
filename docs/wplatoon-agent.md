# How WplatoonAgent is intended to work

`WplatoonAgent` is a separate Windows Phone 8 scheduled-agent project. Its intended job is to run periodically in the background and refresh the pinned tile with the current Splatoon 3 Turf War rotation, even when the user has not opened the app recently.

This describes both the design visible in the source and an important reality: **the agent is unfinished in the current repository and should not be treated as working background tile support.**

## Intended run

The entry point is `ScheduledAgent.OnInvoke` in `WplatoonAgent/WplatoonAgent/ScheduledAgent.cs`. It is designed to:

1. Stop without changing the tile when the saved game choice is Splatoon 2. The agent only knows the Splatoon 3 schedule format.
2. Read its own `schedules3.json` cache. If that cache is less than an hour old, use it directly.
3. Otherwise request `https://splatoon3.ink/data/schedules.json`.
4. If the request fails, fall back to the previously saved JSON when one is available.
5. Find the first upcoming regular Turf War rotation, then pass the rule, two stages, end time, and stage image to a tile updater.
6. Signal completion to Windows Phone with `NotifyComplete`.

The agent's `CacheHelper.cs` stores data in isolated storage and tracks its saved time in application settings. It is separate source from the main app's cache helper, even though the two serve a similar purpose.

## What is incomplete today

- `HomePage.xaml.cs` defines an `EnsureAgent` method that would register a `PeriodicTask`, but there is no call to that method in the checked-in source. So the app does not currently register this scheduled agent through that code path.
- `WplatoonAgent/WplatoonAgent/TileUpdater.cs` is an empty class. `ScheduledAgent` calls `TileUpdater.Update(...)`, but that method is not implemented in the agent project. As checked in, this is a source-level mismatch and the agent project may fail to compile.
- The main app has a different `Wplatoon.TileUpdater` implementation for foreground updates. It does not make the empty agent updater work automatically.
- The agent requests Splatoon 3 data only; it deliberately does nothing in Splatoon 2 mode.

In short: the foreground app's tile update and the scheduled background agent are distinct pieces. The former is implemented in the main app; the latter needs integration and a real updater before it can be relied on.

## If you are completing the agent

Keep the phone's periodic-agent time and resource limits in mind. The agent should always finish its invocation, handle network failure using valid cached data when possible, and avoid presenting stale data as fresh. Coordinate registration with the app lifecycle and confirm the project reference/build setup instead of assuming that adding the project to the solution wires it into the app.

The endpoint and user-agent policy should also be reviewed against the upstream [Splatoon 3.ink data-access notes](https://github.com/misenhower/splatoon3.ink/wiki/Data-Access). After implementation, verify both the agent project build and tile behavior on a supported Windows Phone target; a normal app launch alone does not test scheduled execution.
