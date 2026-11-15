# Wplatoon documentation

Welcome! This is the human-written guide to the repository. You do not need to understand Windows Phone internals to start with the app guide; the architecture and agent notes go deeper for people who want to work on the code.

## The documents

| Document | What it answers |
| --- | --- |
| [User guide](./user-guide.md) | What the app shows and how to use its main screens |
| [Architecture](./architecture.md) | How the main Wplatoon app is organized and how data reaches the screens |
| [Data and caching](./data-and-caching.md) | Which upstream feeds the app uses, when it requests them, and what it saves |
| [Development](./development.md) | What is needed to build the legacy app and how to make a focused contribution |
| [WplatoonAgent](./wplatoon-agent.md) | What the background agent is meant to do and what is unfinished today |

There are also two entry points in the repository:

- [Main README](../README.md): project introduction, features, and a quick build overview.
- [WplatoonAgent README](../WplatoonAgent/README.md): a brief introduction to the agent, linking back here for its detailed notes.

## A quick map

- `Wplatoon/` contains the Windows Phone app: screens, parsers, settings, cache, tile and wallpaper helpers, assets, and its project file.
- `WplatoonAgent/WplatoonAgent/` contains the scheduled background-agent source and project.
- `Wplatoon.sln` opens both projects together in Visual Studio.
- `screenshots/` contains images used by the project presentation.
- `packages/` contains legacy package files used by the projects; it is not application source code.

The app targets Windows Phone 8 Silverlight. Its project files and tooling are from an older Windows Phone development era; see [Development](./development.md) before trying to build it with a modern SDK.
