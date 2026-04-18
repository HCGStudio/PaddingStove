# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

PaddingStove is a Hearthstone deck tracker for iPad. Because iOS sandboxing prevents apps from reading other apps' logs, the architecture offloads log reading to a desktop host on the same Wi-Fi: the host reads Hearthstone's syslog stream from the iPad over `libimobiledevice`, parses game state, and serves a tracker overlay (web page) back to the iPad.

Data flow: iPad (Hearthstone log via syslog_relay) → Host (parse) → iPad (web overlay via SSE).

## Common commands

First-time setup — the `submodules/HearthDb` submodule is referenced as a `<ProjectReference>` from Core and the build will fail without it:
```shell
git submodule update --init --recursive
```

Run the full app (backend serves the built front-end as static files + SPA fallback). The Hosting project has an MSBuild target that runs `yarn install && yarn build` and copies `front-end/dist` into `wwwroot/` automatically:
```shell
dotnet run --project HCGStudio.PaddingStove.Hosting -- --urls http://0.0.0.0:8080
# skip the front-end build step (e.g. no Node installed):
dotnet run --project HCGStudio.PaddingStove.Hosting -p:SkipFrontEndBuild=true -- --urls http://0.0.0.0:8080
```

Front-end dev (Parcel dev server, expects backend on default port with CORS `dev` policy enabled in Development):
```shell
cd front-end
yarn install
yarn start            # parcel dev server
yarn build            # rimraf dist + parcel production build
```

Backend in dev mode (enables OpenAPI at `/openapi` and CORS `dev` policy):
```shell
dotnet run --project HCGStudio.PaddingStove.Hosting
```

Tests (xUnit, currently covers the log-parser regexes in `HCGStudio.PaddingStove.Core.Tests`):
```shell
dotnet test
dotnet test --filter "FullyQualifiedName~GameBoardRegexTests"   # single class
```

Native dependency: `libimobiledevice` must be installed on the host (`brew install libimobiledevice` on macOS). Windows is WIP.

The iPad must have `utils/log.config` placed in the Hearthstone app's Documents directory. The host can push this automatically via `POST /api/Device/{id}/install-log-config` (used by the **Install log.config on Hearthstone** action in the device picker), which opens `com.apple.mobile.house_arrest`, sends `VendDocuments` for `unity.Blizzard Entertainment.Hearthstone`, and writes the embedded `log.config` over AFC. Wi-Fi sync must also be enabled so the host can reach the iPad wirelessly after unplugging.

## Architecture

Two .NET 9 projects plus a Parcel/React front-end:

- **HCGStudio.PaddingStove.Core** — domain logic. No ASP.NET dependency. Entry point is `AddPaddingStoveCore()` (DI extension), which warm-starts `LibIMobileDevice.GetDeviceList()` and registers `IDeviceProvider` and `IGameBoardFactory` as singletons.
- **HCGStudio.PaddingStove.Hosting** — ASP.NET Core Web API + static file host. `Program.cs` wires controllers with source-generated JSON (`JsonContext`), serves `wwwroot` (the built front-end), and falls back to `index.html` for SPA routing.
- **front-end** — React 19 + Tailwind CSS + Radix UI + SWR, bundled with Parcel. Source entry `src/index.html` → `src/index.tsx`. Built output lives in `front-end/dist`; the hosting project serves `wwwroot` (build output must land there for production).

### Cross-boundary type isolation

`Core` exposes internal domain types (`GameBoardStatus`, `DeckContent`, `BoardCounter`, `DeviceInfo`, `GameState`, `DeviceType`). The `Hosting/External/` folder defines parallel DTO types with the same shape, and controllers explicitly map domain → external via `ToExternal(...)`. **Do not leak Core types directly through controller responses** — preserve this mapping when adding new fields, and register any new DTOs in `JsonContext` (source-generated `JsonSerializerContext` for AOT-friendly serialization).

### GameBoard: log → state pipeline

`GameBoard` (in Core) is the heart. It runs two concurrent tasks per device:
1. `WatchLog` reads lines from the iPad's syslog stream (filtered by `"hearthstone"`), runs each through four regex parsers (`ZoneChangeRegex`, `DeckStartRegex`, `GameStartRegex`, `GameEndRegex`), and writes `IBoardChange` events to an unbounded `Channel`.
2. `ProcessGameState` consumes the channel and mutates `PlayerDeck`/`OpponentDeck` dictionaries based on zone transitions (e.g. `Friendly + Deck → -1`, `Opposing + Hand/Deck → Play → +1`). Each mutation triggers `OnStatusChanged`.

`IBoardChange` is a `[Closed]` interface (ExhaustiveMatching.Analyzer); the switch in `ProcessGameState` ends with `ExhaustiveMatch.Failed(state)`. **When you add a new change type, add it to the `[Closed(...)]` attribute and the switch — the analyzer will flag missing cases.**

`GameBoardFactory` caches one `GameBoard` per device id in a `ConcurrentDictionary` (keyed by lockdown UDID); each board owns a `DeviceConnector` → `LockDownClient` → `syslog_relay` service chain (see `Core/Native/`).

### Real-time delivery

`TrackerController.GetAsync` (route `/api/Tracker/{id}`) opens an SSE stream, subscribes to the board's `OnStatusChanged`, sends an initial snapshot, then loops on a 5-second `KeepAliveEvent`. `AsyncLock` serializes writes to the response body so status events and keep-alives don't interleave. SSE payloads are typed as `External.ISseEvent` and serialized through `JsonContext`.

`DeviceController` (`/api/Device`) lists currently connected iOS devices via `IDeviceProvider`.

### Native interop caveat

`Core/Native/LibIMobileDevice.cs` resolves the native library via a custom `NativeLibrary.SetDllImportResolver`. The macOS branch hardcodes `/opt/homebrew/lib/` — fine for Apple Silicon Homebrew, broken for Intel Homebrew (`/usr/local/lib/`) or system installs. Update this resolver when touching native loading or adding bundled binaries.

## Open work

`TODOS.md` tracks deferred items — consult it before starting parser work, native bundling, or .NET-upgrade follow-ups (e.g. the CA2024 fix at `HCGStudio.PaddingStove.Core/GameBoard.cs:47`). Parser additions (mulligan, secrets, mana, hero/health) need real `Power.log` / `Zone.log` samples from a live game; guessing produces parsers that look plausible but silently misfire.
