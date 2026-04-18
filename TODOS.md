# TODOs

Open work items deferred from earlier passes. The completed items are in git history.

## Parser features (need real Hearthstone log samples)

These extend `GameBoard`'s log → state pipeline. All four require capturing actual `Power.log` / `Zone.log` lines from a live game to write correct regexes — guessing produces parsers that look plausible but silently misfire.

### Track and expose hero / health / hero power
The zone parser already recognizes `PLAY (Hero)` / `PLAY (Hero Power)`, but `GameBoardStatus` never surfaces hero card id, hero power id, or current health. Extend the domain type, the External DTO, register it in `JsonContext`, and render in the front-end. Hero health changes come through `TAG_CHANGE Entity=… tag=HEALTH value=…` in `Power.log`.

### Mana tracking (parser + DTO + UI)
Parse current and max mana from `TAG_CHANGE` lines (`tag=RESOURCES`, `tag=RESOURCES_USED`). Add to `GameBoardStatus`, External DTO, `JsonContext`, and a small UI strip.

### Mulligan tracking
Capture starting-hand decisions before `GameStartChange`. Add a new `IBoardChange` variant — remember to update both the `[Closed(...)]` attribute on `IBoardChange` and the exhaustive `switch` in `ProcessGameState` (the analyzer will flag missing cases).

### Secret tracking
Track secrets played by both players. Add an `IBoardChange` variant and a list field on `GameBoardStatus`; render pending opponent secrets in the UI.

## Other

### Windows libimobiledevice support
Currently flagged WIP in the README. Either bundle native binaries via a NuGet runtime package (preferred) or document a manual install path. Note: the macOS resolver in `Core/Native/LibIMobileDevice.cs` hardcodes `/opt/homebrew/lib/` — bundle macOS too while you're at it.

## Followups noticed during the .NET 10 upgrade

### CA2024 in `GameBoard.WatchLog`
`HCGStudio.PaddingStove.Core/GameBoard.cs:47` uses `while (!_logStream.EndOfStream)` inside an async method. The .NET 10 analyzer warns because `EndOfStream` may block on the underlying stream. Switch to looping on `ReadLineAsync` returning `null`.

### Replace `Assert.True/False` over regex with `Assert.Matches`/`DoesNotMatch`
`HCGStudio.PaddingStove.Core.Tests/GameBoardRegexTests.cs` triggers four `xUnit2008` warnings. Cosmetic — improves diagnostic output on failure.

### Front-end yarn workflow
`yarn build` now succeeds against the regenerated lockfile, but the `package.json` still has `"add": "^2.0.6"` removed — confirm no script outside `src/` (e.g. a hidden config) depended on it.
