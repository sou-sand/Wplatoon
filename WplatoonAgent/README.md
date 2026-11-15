# WplatoonAgent

This project is the Windows Phone 8 scheduled-agent companion to the Wplatoon app. It is intended to refresh the pinned tile with the current Splatoon 3 Turf War rotation while the app is in the background.

The agent is **unfinished in the current source**: the app does not call its task-registration method, and the agent project contains an empty tile-updater class even though `ScheduledAgent` calls an updater method. Do not assume background tile refresh is currently functional.

For the intended flow, source map, and implementation caveats, see the [WplatoonAgent guide](../docs/wplatoon-agent.md). For the complete repository guide, start at [the documentation index](../docs/README.md).