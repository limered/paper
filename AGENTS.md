# AGENTS.md

Godot 4.7.0 origami simulator written in C#. Two projects in one solution: the game (`valleyfold/`) and xUnit tests (`Testing/`).

## Layout

- `valleyfold/` — Godot project. SDK `Godot.NET.Sdk/4.7.0`, `net8.0`. Main scene `res://scenes/main.tscn` (Hub entry point). Source under `valleyfold/src/`.
- `Testing/` — xUnit + NSubstitute test project on `net8.0`. References `valleyfold.csproj`, so tests can import Godot types but must not touch the engine runtime (no `_Ready`, no scene tree).
- `valleyfold/valleyfold.sln` is the solution file for both projects.

## Build & test

- Build C#: `dotnet build valleyfold/valleyfold.sln`. The Godot editor also regenerates this on save — avoid editing `.csproj`/`.sln` by hand if a Godot session is open.
- There is no lint/format config and no CI. Don't invent commands.
- Running the game itself requires the Godot 4.7.0 .NET editor; don't try to launch it headlessly from an agent session.

## Test harness

The factory test phase runs every `test-harness.<name>` command declared here. Any failure fails the run.

test-harness.tests: dotnet test valleyfold/valleyfold.sln

Single test: `dotnet test --filter "FullyQualifiedName~VertexToVertexFoldTests"`.

## Architecture quirks an agent will miss

- **Global mutable state lives in `valleyfold/src/Statics.cs`.** `Statics.Frame`, `Statics.Game`, `Statics.Frame3d`, `Statics.ChangeMemory`, `Statics.FoldInteractionApplier` are singletons set during `Game._Ready`. Most subsystems reach into them rather than receiving dependencies. Tests construct a local `Frame` directly and avoid `Statics` — keep new code testable the same way (see `Testing/Folding/*`).
- **Communication is via a static pub/sub `EventBus`** (`valleyfold/src/Utils/EventBus.cs`). `Register<T>` / `Emit<T>` / `Deregister<T>`. Event payload types live under `*/Events/` folders (e.g. `Ui/Events`, `Render/ThreeDee/Events`). New cross-cutting interactions should go through the bus, not direct node references.
- **Two parallel models:** `TwoDeeModels/Frame` is the authoritative flat paper graph (vertices/edges/faces with `Id`s); `ThreeDeeModels/Frame3D` mirrors it for rendering and is rebuilt via `ImportFromFrame`. Mutations happen on `Frame`, then `Frame3d.ImportFromFrame(Statics.Frame)` syncs.
- **Folds** implement `IFold` in `valleyfold/src/Folding/` (`VertexToVertexFold`, `VertexToEdgeFold`, `EdgeToEdgeFold`). They mutate a `Frame` in place via `Apply(frame)`. `FoldInteractionApplier` is the entry point from UI/interaction code.
- `FrameModifications/` holds command/query helpers operating on `Frame` — prefer reusing these over inlining graph traversal.
- `ChangeTracking/ChangeMemory` records edits for undo; `Frame.AddEdgeAfterSplit` already pushes into it via `Statics.ChangeMemory`. New mutating ops should record changes the same way.

## Conventions

- Namespaces mirror folders under `valleyfold.*` and `Testing.*`. File-scoped namespaces are used throughout.
- Test classes follow nested-class-per-scenario style (`class SimpleFold : VertexToVertexFoldTests { [Fact] ... }`) — match it when adding tests.
- `.gitignore` excludes `.godot/`, `Testing/bin`, `Testing/obj`. Generated Godot import files (`*.import`) are committed; don't delete them.

## Agent skills

### Issue tracker

GitHub Issues — `https://github.com/limered/paper/issues`.

### Triage labels

AFK-ready issues are labelled `ready-for-agent`; human-only work is `ready-for-human`.

### Domain docs

ADRs under `docs/adr/`.
